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

using System.Threading.Tasks;
#if NETSTANDARD2_0 || NET461
using Microsoft.Extensions.Configuration;
#endif
using Platibus.Security;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// A factory for initializing a <see cref="IMessageEncryptionService"/> during bus initialization.
    /// </summary>
    public interface IMessageEncryptionServiceProvider
    {
        /// <summary>
        /// Creates and initializes a <see cref="IMessageEncryptionService"/>
        /// based on the provided <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The encruption configuration element.</param>
        /// <returns>Returns a task whose result is an initialized
        /// <see cref="IMessageEncryptionService"/>.</returns>
#if NET452 || NET461
        Task<IMessageEncryptionService> CreateMessageEncryptionService(EncryptionElement configuration);
#endif
#if NETSTANDARD2_0 || NET461
        Task<IMessageEncryptionService> CreateMessageEncryptionService(IConfiguration configuration);
#endif
    }
}
