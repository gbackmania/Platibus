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
using System.Text;
using System.Threading.Tasks;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Factory class used to initialize <see cref="RabbitMQHostConfiguration"/> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class RabbitMQHostConfigurationManager : PlatibusConfigurationManager<RabbitMQHostConfiguration>
    {
#if NET452
        /// <inheritdoc />
        public override async Task Initialize(RabbitMQHostConfiguration configuration, string configSectionName = null)
        {
            var diagnosticsService = configuration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(configSectionName))
            {
                configSectionName = "platibus.rabbitmq";
                await diagnosticsService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default configuration section \"" + configSectionName + "\""
                    }.Build());
            }

            var configSection = LoadConfigurationSection<RabbitMQHostConfigurationSection>(configSectionName, diagnosticsService);
            await Initialize(configuration, configSection);
        }

        /// <summary>
        /// Initializes the supplied HTTP server <paramref name="configuration"/> based on the
        /// properties of the provided <paramref name="configSection"/>
        /// </summary>
        /// <param name="configuration">The configuration to initialize</param>
        /// <param name="configSection">The configuration section whose properties are to be used
        /// to initialize the <paramref name="configuration"/></param>
        /// <returns>Returns a task that completes when the configuration has been initialized</returns>
        public async Task Initialize(RabbitMQHostConfiguration configuration,
            RabbitMQHostConfigurationSection configSection)
        {
            await base.Initialize(configuration, configSection);

            configuration.BaseUri = configSection.BaseUri ?? new Uri(RabbitMQDefaults.BaseUri);

            configuration.Encoding = string.IsNullOrWhiteSpace(configSection.Encoding)
                ? Encoding.UTF8
                : Encoding.GetEncoding(configSection.Encoding);

            configuration.AutoAcknowledge = configSection.AutoAcknowledge;
            configuration.ConcurrencyLimit = configSection.ConcurrencyLimit;
            configuration.MaxAttempts = configSection.MaxAttempts;
            configuration.RetryDelay = configSection.RetryDelay;
            configuration.IsDurable = configSection.IsDurable;

            var securityTokenServiceFactory = new SecurityTokenServiceFactory(configuration.DiagnosticService);
            var securityTokenConfig = configSection.SecurityTokens;
            configuration.SecurityTokenService = await securityTokenServiceFactory.InitSecurityTokenService(securityTokenConfig);
        }

        /// <summary>
        /// Initializes and returns a <see cref="RabbitMQHostConfiguration"/> instance based on
        /// the <see cref="RabbitMQHostConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus.rabbitmq")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <see cref="PlatibusConfiguration"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        public new static async Task<RabbitMQHostConfiguration> LoadConfiguration(string sectionName = "platibus.rabbitmq", 
            bool processConfigurationHooks = true)
        {
            var configManager = new RabbitMQHostConfigurationManager();
            var configuration = new RabbitMQHostConfiguration();
            await configManager.Initialize(configuration, sectionName);
            if (processConfigurationHooks)
            {
                await configManager.FindAndProcessConfigurationHooks(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Helper method to initialize security token services based on the
        /// supplied configuration element
        /// </summary>
        /// <param name="config">The security tokens configuration element</param>
        /// <returns>Returns a task whose result is an initialized security token service</returns>
        [Obsolete("Use SubscriptionTrackingServiceFactory.InitSubscriptionTrackingService")]
        public static Task<ISubscriptionTrackingService> InitSubscriptionTrackingService(
            SubscriptionTrackingElement config)
        {
            var factory = new SubscriptionTrackingServiceFactory();
            return factory.InitSubscriptionTrackingService(config);
        }
#endif
#if NETSTANDARD2_0
        /// <inheritdoc />
        public override async Task Initialize(RabbitMQHostConfiguration platibusConfiguration, string configSectionName = null)
        {
            var diagnosticsService = platibusConfiguration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(configSectionName))
            {
                configSectionName = "platibus.rabbitmq";
                await diagnosticsService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default configuration section \"" + configSectionName + "\""
                    }.Build());
            }

            var configSection = LoadConfigurationSection(configSectionName, diagnosticsService);
            await Initialize(platibusConfiguration, configSection);
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes the supplied HTTP server <paramref name="platibusConfiguration" /> based on the
        /// properties of the provided <paramref name="configuration" />
        /// </summary>
        /// <param name="platibusConfiguration">The configuration to initialize</param>
        /// <param name="configuration">The configuration section whose properties are to be used
        /// to initialize the <paramref name="platibusConfiguration" /></param>
        /// <returns>Returns a task that completes when the configuration has been initialized</returns>
        public override async Task Initialize(RabbitMQHostConfiguration platibusConfiguration, IConfiguration configuration)
        {
            await base.Initialize(platibusConfiguration, configuration);

            var defaultBaseUri = new Uri(RabbitMQDefaults.BaseUri);
            platibusConfiguration.BaseUri = configuration?.GetValue<Uri>("baseUri") ?? defaultBaseUri;

            var encodingName = configuration?["encoding"] ?? RabbitMQDefaults.Encoding;
            platibusConfiguration.Encoding = string.IsNullOrWhiteSpace(encodingName)
                ? Encoding.UTF8
                : Encoding.GetEncoding(encodingName);

            platibusConfiguration.AutoAcknowledge = configuration?.GetValue("autoAcknowledge", RabbitMQDefaults.AutoAcknowledge) ?? RabbitMQDefaults.AutoAcknowledge;
            platibusConfiguration.ConcurrencyLimit = configuration?.GetValue("concurrencyLimit", RabbitMQDefaults.ConcurrencyLimit) ?? RabbitMQDefaults.ConcurrencyLimit;
            platibusConfiguration.MaxAttempts = configuration?.GetValue("maxAttempts", RabbitMQDefaults.MaxAttempts) ?? RabbitMQDefaults.MaxAttempts;

            var defaultRetryDelay = TimeSpan.Parse(RabbitMQDefaults.RetryDelay);
            platibusConfiguration.RetryDelay = configuration?.GetValue("retryDelay", defaultRetryDelay) ?? defaultRetryDelay;
            platibusConfiguration.IsDurable = configuration?.GetValue("durable", RabbitMQDefaults.Durable) ?? RabbitMQDefaults.Durable;

            var securityTokenServiceFactory = new SecurityTokenServiceFactory(platibusConfiguration.DiagnosticService);
            var securityTokensSection = configuration?.GetSection("securityTokens");
            platibusConfiguration.SecurityTokenService = await securityTokenServiceFactory.InitSecurityTokenService(securityTokensSection);
        }
#endif
    }
}
