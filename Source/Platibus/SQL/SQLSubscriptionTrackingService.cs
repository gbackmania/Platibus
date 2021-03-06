﻿// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;
using Platibus.SQL.Commands;
#if NET452 || NET461
using System.Configuration;
#endif
#if NETSTANDARD2_0
using Platibus.Config;
#endif

namespace Platibus.SQL
{
    /// <summary>
    /// A <see cref="ISubscriptionTrackingService"/> implementation that uses a SQL database to
    /// store queued messages
    /// </summary>
    public class SQLSubscriptionTrackingService : ISubscriptionTrackingService, IDisposable
    {
        /// <summary>
        /// A data sink provided by the implementer to handle diagnostic events
        /// </summary>
        protected readonly IDiagnosticService DiagnosticService;

        private readonly ISubscriptionTrackingCommandBuilders _commandBuilders;

        private readonly ConcurrentDictionary<TopicName, IEnumerable<SQLSubscription>> _subscriptions =
            new ConcurrentDictionary<TopicName, IEnumerable<SQLSubscription>>();

        private bool _disposed;

        /// <summary>
        /// The connection provider used to obtain connections to the SQL database
        /// </summary>
        public IDbConnectionProvider ConnectionProvider { get; }

        /// <summary>
        /// Initializes a new <see cref="SQLSubscriptionTrackingService"/> with the specified connection
        /// string settings and dialect
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings to use to connect to
        ///     the SQL database</param>
        /// <param name="commandBuilders">(Optional) A collection of factories capable of 
        ///     generating database commands for manipulating subscriptions that conform to the SQL
        ///     syntax required by the underlying connection provider (if needed)</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        ///     are reported and processed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringSettings"/>
        ///     is <c>null</c></exception>
        /// <remarks>
        /// If a SQL dialect is not specified, then one will be selected based on the supplied
        /// connection string settings
        /// </remarks>
        /// <seealso cref="ISubscriptionTrackingCommandBuildersProvider"/>
        public SQLSubscriptionTrackingService(ConnectionStringSettings connectionStringSettings, ISubscriptionTrackingCommandBuilders commandBuilders = null, IDiagnosticService diagnosticService = null)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException(nameof(connectionStringSettings));
            DiagnosticService = diagnosticService ?? Diagnostics.DiagnosticService.DefaultInstance;
            ConnectionProvider = new DefaultConnectionProvider(connectionStringSettings, DiagnosticService);
            _commandBuilders = commandBuilders ??
                               new CommandBuildersFactory(connectionStringSettings, DiagnosticService)
                                   .InitSubscriptionTrackingCommandBuilders();
        }

        /// <summary>
        /// Initializes a new <see cref="SQLSubscriptionTrackingService"/> with the specified connection
        /// provider and dialect
        /// </summary>
        /// <param name="connectionProvider">The connection provider to use to connect to
        ///     the SQL database</param>
        /// <param name="commandBuilders">A collection of factories capable of 
        ///     generating database commands for manipulating subscriptions that conform to the SQL
        ///     syntax required by the underlying connection provider</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        ///     are reported and processed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionProvider"/>
        /// or <paramref name="commandBuilders"/> is <c>null</c></exception>
        public SQLSubscriptionTrackingService(IDbConnectionProvider connectionProvider, 
            ISubscriptionTrackingCommandBuilders commandBuilders, 
            IDiagnosticService diagnosticService = null)
        {
            DiagnosticService = diagnosticService ?? Diagnostics.DiagnosticService.DefaultInstance;
            ConnectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _commandBuilders = commandBuilders ?? throw new ArgumentNullException(nameof(commandBuilders));
        }

        /// <summary>
        /// Initializes the subscription tracking service by creating the necessary objects in the
        /// SQL database
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void Init()
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                var commandBuilder = _commandBuilders.NewCreateObjectsCommandBuilder();
                using (var command = commandBuilder.BuildDbCommand(connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }

            var subscriptionsByTopicName = SelectSubscriptions()
                .GroupBy(subscription => subscription.TopicName);

            foreach (var grouping in subscriptionsByTopicName)
            {
                var topicName = grouping.Key;
                var subscriptions = grouping.ToList();
                _subscriptions.AddOrUpdate(topicName, subscriptions, (t, s) => s.Union(subscriptions).ToList());
            }
        }

        /// <summary>
        /// Adds or updates a subscription
        /// </summary>
        /// <param name="topic">The topic to which the <paramref name="subscriber"/> is
        /// subscribing</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <param name="ttl">(Optional) The maximum Time To Live (TTL) for the subscription</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel the addition of the subscription</param>
        /// <returns>Returns a task that will complete when the subscription has been added or
        /// updated</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topic"/> or
        /// <paramref name="subscriber"/> is <c>null</c></exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed</exception>
        public async Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = default(TimeSpan),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));

            CheckDisposed();
            var expires = ttl <= TimeSpan.Zero ? DateTime.MaxValue : (DateTime.UtcNow + ttl);
            cancellationToken.ThrowIfCancellationRequested();
            var subscription = await InsertOrUpdateSubscription(topic, subscriber, expires);
            _subscriptions.AddOrUpdate(topic, new[] {subscription},
                (t, existing) => new[] {subscription}.Union(existing).ToList());
        }

        /// <summary>
        /// Removes a subscription
        /// </summary>
        /// <param name="topic">The topic to which the <paramref name="subscriber"/> is
        /// subscribing</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel the subscription removal</param>
        /// <returns>Returns a task that will complete when the subscription has been removed</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topic"/> or
        /// <paramref name="subscriber"/> is <c>null</c></exception>
        /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed</exception>
        public async Task RemoveSubscription(TopicName topic, Uri subscriber,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));

            CheckDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            await DeleteSubscription(topic, subscriber);

            _subscriptions.AddOrUpdate(topic, new SQLSubscription[0],
                (t, existing) => existing.Where(se => se.Subscriber != subscriber).ToList());
        }

        /// <summary>
        /// Returns a list of the current, non-expired subscriber URIs for a topic
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// to cancel the query</param>
        /// <returns>Returns a task whose result is the distinct set of base URIs of all Platibus
        /// instances subscribed to the specified local topic</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topic"/> is <c>null</c>
        /// </exception>
        public Task<IEnumerable<Uri>> GetSubscribers(TopicName topic,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            
            CheckDisposed();
            _subscriptions.TryGetValue(topic, out IEnumerable<SQLSubscription> subscriptions);
            var activeSubscribers = (subscriptions ?? Enumerable.Empty<SQLSubscription>())
                .Where(s => s.Expires > DateTime.UtcNow)
                .Select(s => s.Subscriber);

            return Task.FromResult(activeSubscribers);
        }

        /// <summary>
        /// Inserts or updates a subscription record in the SQL database
        /// </summary>
        /// <param name="topic">The topic to which the <paramref name="subscriber"/> is subscribing</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <param name="expires">The date and time at which the subscription will expire</param>
        /// <returns>Returns a task that will complete when the subscription record has been inserted 
        /// or updated and whose result will be the an immutable representation of the inserted 
        /// subscription record</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual Task<SQLSubscription> InsertOrUpdateSubscription(TopicName topic, Uri subscriber,
            DateTime expires)
        {
            SQLSubscription subscription;
            var connection = ConnectionProvider.GetConnection();
            try
            {
                var insertBuilder = _commandBuilders.NewInsertSubscriptionCommandBuilder();
                insertBuilder.TopicName = topic;
                insertBuilder.Subscriber = subscriber.ToString();
                insertBuilder.Expires = expires;

                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (var insertCommand = insertBuilder.BuildDbCommand(connection))
                    {
                        var rowsAffected = insertCommand.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            var updateBuilder = _commandBuilders.NewUpdateSubscriptionCommandBuilder();
                            updateBuilder.TopicName = topic;
                            updateBuilder.Subscriber = subscriber.ToString();
                            updateBuilder.Expires = expires;

                            using (var updateCommand = updateBuilder.BuildDbCommand(connection))
                            {
                                updateCommand.ExecuteNonQuery();
                            }
                        }
                    }
                    subscription = new SQLSubscription(topic, subscriber, expires);
                    scope.Complete();
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
            return Task.FromResult(subscription);
        }

        /// <summary>
        /// Selects all of the non-expired subscription records from the SQL database
        /// </summary>
        /// <returns>Returns a task that will complete when the subscription records have been
        /// selected and whose result will be the records that were selected</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual IEnumerable<SQLSubscription> SelectSubscriptions()
        {
            var subscriptions = new List<SQLSubscription>();
            var connection = ConnectionProvider.GetConnection();
            try
            {
                var commandBuilder = _commandBuilders.NewSelectSubscriptionsCommandBuilder();
                commandBuilder.CutoffDate = DateTime.UtcNow;

                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (var command = commandBuilder.BuildDbCommand(connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var topicName = (TopicName) reader.GetString("TopicName");
                                var subscriber = new Uri(reader.GetString("Subscriber"));
                                var expires = reader.GetDateTime("Expires").GetValueOrDefault(DateTime.MaxValue);
                                var subscription = new SQLSubscription(topicName, subscriber, expires);
                                subscriptions.Add(subscription);
                            }
                        }
                    }
                    scope.Complete();
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
            return subscriptions;
        }

        /// <summary>
        /// Deletes a subscription record from the SQL database
        /// </summary>
        /// <param name="topic">The name of the topic</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <returns>Returns a task that will complete when the subscription record
        /// has been deleted</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        protected virtual async Task DeleteSubscription(TopicName topic, Uri subscriber)
        {
            var connection = ConnectionProvider.GetConnection();
            try
            {
                var commandBuilder = _commandBuilders.NewDeleteSubscriptionCommandBuilder();
                commandBuilder.TopicName = topic;
                commandBuilder.Subscriber = subscriber.ToString();

                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (var command = commandBuilder.BuildDbCommand(connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    scope.Complete();
                }
            }
            finally
            {
                ConnectionProvider.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Finalizer method that ensures resources are freed
        /// </summary>
        ~SQLSubscriptionTrackingService()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or by the finalizer to free held resources
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or from the finalizer (<c>false</c>)</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (ConnectionProvider is IDisposable disposableConnectionProvider)
                {
                    disposableConnectionProvider.Dispose();
                }
            }
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the subscription tracking service has
        /// been disposed
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object is already disposed</exception>
        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}