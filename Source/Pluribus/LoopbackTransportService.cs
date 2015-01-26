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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Pluribus
{
    /// <summary>
    /// Transport service that sends all messages to the sender.
    /// </summary>
    /// <remarks>
    /// Useful for in-process message passing within a single application.
    /// </remarks>
    public class LoopbackTransportService : ITransportService
    {
        public event MessageReceivedHandler MessageReceived;
        public event SubscriptionRequestReceivedHandler SubscriptionRequestReceived;

        public async Task SendMessage(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            await AcceptMessage(message, Thread.CurrentPrincipal, cancellationToken);
        }

        public Task SendSubscriptionRequest(SubscriptionRequestType requestType, Uri publisher, TopicName topic, Uri subscriber,
            TimeSpan ttl, CancellationToken cancellationToken = new CancellationToken())
        {
            return AcceptSubscriptionRequest(requestType, topic, subscriber, ttl, 
                Thread.CurrentPrincipal, cancellationToken);
        }

        public Task AcceptMessage(Message message, IPrincipal senderPrincipal,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.Run(() =>
            {
                var handlers = MessageReceived;
                if (handlers != null)
                {
                    handlers(this, new MessageReceivedEventArgs(message, senderPrincipal));
                }
            }, cancellationToken);
        }

        public Task AcceptSubscriptionRequest(SubscriptionRequestType requestType, TopicName topic, Uri subscriber, TimeSpan ttl,
            IPrincipal senderPrincipal, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.Run(() =>
            {
                var handlers = SubscriptionRequestReceived;
                if (handlers != null)
                {
                    handlers(this, new SubscriptionRequestReceivedEventArgs(requestType, topic, subscriber, ttl, senderPrincipal));
                }
            }, cancellationToken);
        }
    }
}
