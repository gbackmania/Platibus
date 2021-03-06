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
using System.IO;
using System.Threading.Tasks;

namespace Platibus.Http
{
    /// <summary>
    /// Extension methods for working with HTTP resource requests
    /// </summary>
    public static class HttpResourceRequestExtensions
    {
        /// <summary>
        /// Indicates whether a <paramref name="request"/> is a POST method request
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>Returns <c>true</c> if the request is a POST method request</returns>
        public static bool IsPost(this IHttpResourceRequest request)
        {
            return request != null && "POST".Equals(request.HttpMethod, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Indicates whether a <paramref name="request"/> is a GET method request
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>Returns <c>true</c> if the request is a GET method request</returns>
        public static bool IsGet(this IHttpResourceRequest request)
        {
            return request != null && "GET".Equals(request.HttpMethod, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads the entire content of an HTTP resource request as a string
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>Returns a task whose result is the content of the <paramref name="request"/>
        /// as a string</returns>
        public static async Task<string> ReadContentAsString(this IHttpResourceRequest request)
        {
            if (request == null) return null;

            var contentStream = request.InputStream;
            var contentEncoding = request.ContentEncoding;
            using (var contentReader = new StreamReader(contentStream, contentEncoding))
            {
                return await contentReader.ReadToEndAsync();
            }
        }
    }
}