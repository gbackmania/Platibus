﻿// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using RabbitMQ.Client;

namespace Platibus.RabbitMQ
{
    public class RabbitMQMessageQueueingService : IMessageQueueingService, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(RabbitMQLoggingCategories.RabbitMQ);

        private readonly ConcurrentDictionary<QueueName, RabbitMQQueue> _queues =
            new ConcurrentDictionary<QueueName, RabbitMQQueue>();

        private readonly Encoding _encoding;
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private bool _disposed;

        public RabbitMQMessageQueueingService(Uri uri, Encoding encoding = null)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            _connectionFactory = new ConnectionFactory
            {
                Uri = uri.ToString()
            };
            _encoding = encoding ?? Encoding.UTF8;
        }

        public void Init()
        {
            if (_connection == null)
            {
                _connection = _connectionFactory.CreateConnection();
            }
        }

        public Task CreateQueue(QueueName queueName, IQueueListener listener, QueueOptions options = new QueueOptions())
        {
            CheckDisposed();
            return Task.Run(() =>
            {
                var queue = new RabbitMQQueue(queueName, listener, _connection, _encoding, options);
                if (!_queues.TryAdd(queueName, queue))
                {
                    throw new QueueAlreadyExistsException(queueName);
                }

                Log.DebugFormat("Initializing RabbitMQ queue \"{0}\"", queueName);
                queue.Init();
                Log.DebugFormat("RabbitMQ queue \"{0}\" created successfully", queueName);
                return Task.FromResult(true);
            });
        }

        public async Task EnqueueMessage(QueueName queueName, Message message, IPrincipal senderPrincipal)
        {
            CheckDisposed();
            RabbitMQQueue queue;
            if (!_queues.TryGetValue(queueName, out queue)) throw new QueueNotFoundException(queueName);

            Log.DebugFormat("Enqueueing message ID {0} in RabbitMQ queue \"{1}\"...", message.Headers.MessageId,
                queueName);
            await queue.Enqueue(message, senderPrincipal);
            Log.DebugFormat("Message ID {0} enqueued successfully in RabbitMQ queue \"{1}\"", message.Headers.MessageId,
                queueName);
        }

        public void DeleteQueue(QueueName queueName)
        {
            CheckDisposed();
            RabbitMQQueue queue;
            if (!_queues.TryRemove(queueName, out queue)) return;

            queue.Delete();
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~RabbitMQMessageQueueingService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                foreach (var queue in _queues.Values)
                {
                    queue.Dispose();
                }
                _connection.Close();
            }
            _disposed = true;
        }
    }
}