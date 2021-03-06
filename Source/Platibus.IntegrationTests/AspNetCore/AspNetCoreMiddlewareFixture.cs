﻿#if NETCOREAPP2_0
// The MIT License (MIT)
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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Platibus.Config;
using System;

namespace Platibus.IntegrationTests.AspNetCore
{
    public class AspNetCoreMiddlewareFixture : IDisposable
    {
        private bool _disposed;

        private readonly AspNetCoreSelfHost _sendingHost;
        private readonly AspNetCoreSelfHost _receivingHost;

        public IBus Sender => _sendingHost.Bus;
        public IBus Receiver => _receivingHost.Bus;

        public AspNetCoreMiddlewareFixture()
        {
            _sendingHost = AspNetCoreSelfHost.Start("platibus.aspnetcore0");
            _receivingHost = AspNetCoreSelfHost.Start("platibus.aspnetcore1", configuration =>
            {
                configuration.AddHandlingRule<TestMessage>(".*TestMessage", TestHandler.HandleMessage, "TestHandler");
                configuration.AddHandlingRule(".*TestPublication", new TestPublicationHandler(), "TestPublicationHandler");
            });
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
            _sendingHost?.Dispose();
            _receivingHost?.Dispose();
        }
    }
}
#endif