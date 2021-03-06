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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Security;

namespace Platibus.Http.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// An HTTP resource controller for messages and related resources
    /// </summary>
    public class MessageController : IHttpResourceController
    {
        private readonly Func<Message, IPrincipal, CancellationToken, Task> _accept;
        private readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// Initializes a new <see cref="MessageController"/>
        /// </summary>
        /// <param name="accept">A callback inokved when a message resource is posted</param>
        /// <param name="authorizationService">(Optional) Used to determine whether
        /// a requestor is authorized to post messages</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="accept"/> is
        /// <c>null</c></exception>
        public MessageController(Func<Message, IPrincipal, CancellationToken, Task> accept, IAuthorizationService authorizationService = null)
        {
            _accept = accept ?? throw new ArgumentNullException(nameof(accept));
            _authorizationService = authorizationService;
        }

        /// <inheritdoc />
        public async Task Process(IHttpResourceRequest request, IHttpResourceResponse response, IEnumerable<string> subPath)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));

            if (!request.IsPost())
            {
                response.StatusCode = 405;
                response.AddHeader("Allow", "POST");
                return;
            }

            await Post(request, response);
        }

        private async Task Post(IHttpResourceRequest request, IHttpResourceResponse response)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));

            var authorized = _authorizationService == null ||
                             await _authorizationService.IsAuthorizedToSendMessages(request.Principal);

            if (!authorized)
            {
                response.StatusCode = 401;
                response.StatusDescription = "Unauthorized";
                return;
            }

            // The authorization header should have been processed by the HTTP host prior to this
            // point in order to initialize the principal on the request context.  This is
            // sensitive information and should not be copied into and thus exposed by the message 
            // headers.
            var authorizationHeader = HttpRequestHeader.Authorization.ToString("G");
            var sensitiveHeaders = new[] {authorizationHeader};

            var flattenedHeaders = request.Headers.AllKeys
                .Except(sensitiveHeaders, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k, k => request.Headers[k]);

            var messageHeaders = new MessageHeaders(flattenedHeaders)
            {
                Received = DateTime.UtcNow
            };
            
            var content = await request.ReadContentAsString();
            var message = new Message(messageHeaders, content);
            Thread.CurrentPrincipal = request.Principal;
            await _accept(message, request.Principal, default(CancellationToken));
            response.StatusCode = 202;
        }
    }
}