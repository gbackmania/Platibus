﻿using System;
using Platibus.Diagnostics;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// A <see cref="DiagnosticEvent"/> with additional fields specific to RabbitMQ operations
    /// </summary>
    public class RabbitMQEvent : DiagnosticEvent
    {
        private readonly string _exchange;
        private readonly int? _channelNumber;
        private readonly string _consumerTag;
        private readonly ulong? _deliveryTag;

        /// <summary>
        /// The exchange to which the event pertains, if applicable
        /// </summary>
        public string Exchange => _exchange;

        /// <summary>
        /// The channel number to which the event pertains, if applicable
        /// </summary>
        public int? ChannelNumber => _channelNumber;

        /// <summary>
        /// The consumer tag to which the event pertains, if applicable
        /// </summary>
        public string ConsumerTag => _consumerTag;

        /// <summary>
        /// The delivery tag to which the event pertains, if applicable
        /// </summary>
        public ulong? DeliveryTag => _deliveryTag;

        /// <summary>
        /// Initializes a new <see cref="RabbitMQEvent"/>
        /// </summary>
        /// <param name="source">The object that emitted the event</param>
        /// <param name="type">The type of event</param>
        /// <param name="detail">Specific details regarding this instance of the event</param>
        /// <param name="exception">The exception related to the event, if applicable</param>
        /// <param name="message">The message to which the event pertains, if applicable</param>
        /// <param name="endpoint">The name of the endpoint, if applicable</param>
        /// <param name="queue">The queue to which the event pertains, if applicable</param>
        /// <param name="topic">The topic to which the message pertains, if applicable</param>
        /// <param name="exchange">The exchange to which the event pertains, if applicable</param>
        /// <param name="channelNumber">The channel number to which the message pertains, if applicable</param>
        /// <param name="consumerTag">The consumer tag to which the message pertains, if applicable</param>
        /// <param name="deliveryTag">The delivery tag to which the message pertains, if applicable</param>
        public RabbitMQEvent(object source, DiagnosticEventType type, string detail = null, Exception exception = null, Message message = null, EndpointName endpoint = null, QueueName queue = null, TopicName topic = null, string exchange = null, int? channelNumber = null, string consumerTag = null, ulong? deliveryTag = null) 
            : base(source, type, detail, exception, message, endpoint, queue, topic)
        {
            _exchange = exchange;
            _channelNumber = channelNumber;
            _consumerTag = consumerTag;
            _deliveryTag = deliveryTag;
        }
    }
}
