﻿// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN     NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.Queueing;
using Platibus.Security;
using Platibus.SQLite;
using Xunit;

namespace Platibus.UnitTests.SQLite
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "SQLite")]
    [Collection(AesEncryptedSQLiteCollection.Name)]
    public class SQLiteMessageQueueingServiceTests : MessageQueueingServiceTests<SQLiteMessageQueueingService>
    {
        private readonly DirectoryInfo _queueDirectory;
        private readonly IMessageEncryptionService _messageEncryptionService;
        
        public SQLiteMessageQueueingServiceTests(AesEncryptedSQLiteFixture fixture)
            : base(fixture.DiagnosticService, fixture.MessageQueueingService)
        {
            _queueDirectory = fixture.QueueDirectory;
            _messageEncryptionService = fixture.MessageEncryptionService;
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            using (var queueInspector = new SQLiteMessageQueueInspector(_queueDirectory, queueName, SecurityTokenService, _messageEncryptionService))
            {
                await queueInspector.Init();
                await queueInspector.InsertMessage(new QueuedMessage(message, principal));
            }
        }

        protected override async Task<bool> MessageQueued(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            using (var queueInspector = new SQLiteMessageQueueInspector(_queueDirectory, queueName, SecurityTokenService, _messageEncryptionService))
            {
                await queueInspector.Init();
                var messagesInQueue = await queueInspector.EnumerateMessages();
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }

        protected override async Task<bool> MessageDead(QueueName queueName, Message message)
        {
            var messageId = message.Headers.MessageId;
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddSeconds(-5);
            using (var queueInspector = new SQLiteMessageQueueInspector(_queueDirectory, queueName, SecurityTokenService, _messageEncryptionService))
            {
                await queueInspector.Init();
                var messagesInQueue = await queueInspector.EnumerateAbandonedMessages(startDate, endDate);
                return messagesInQueue.Any(m => m.Message.Headers.MessageId == messageId);
            }
        }
    }
}
