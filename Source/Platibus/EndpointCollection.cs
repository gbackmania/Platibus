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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Platibus.Config;

namespace Platibus
{
    /// <inheritdoc />
    /// <summary>
    /// A mutable <see cref="T:Platibus.IEndpointCollection" /> implementation
    /// </summary>
    public class EndpointCollection : IEndpointCollection
    {
        public static readonly IEndpointCollection Empty = new EndpointCollection();

        private readonly IDictionary<EndpointName, IEndpoint> _endpoints = new Dictionary<EndpointName, IEndpoint>();

        /// <summary>
        /// Adds a named endpoint to the collection
        /// </summary>
        /// <param name="endpointName">The name of the endpoint</param>
        /// <param name="endpoint">The endpoint</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpointName"/>
        /// or <paramref name="endpoint"/> are <c>null</c></exception>
        /// <exception cref="EndpointAlreadyExistsException">Thrown if there is already an
        /// endpoint with the specified <paramref name="endpointName"/></exception>
        public virtual void Add(EndpointName endpointName, IEndpoint endpoint)
        {
            if (endpointName == null) throw new ArgumentNullException(nameof(endpointName));
            if (_endpoints.ContainsKey(endpointName)) throw new EndpointAlreadyExistsException(endpointName);
            _endpoints[endpointName] = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        /// <inheritdoc />
        public virtual IEndpoint this[EndpointName endpointName]
        {
            get
            {
                if (!_endpoints.TryGetValue(endpointName, out IEndpoint endpoint))
                {
                    throw new EndpointNotFoundException(endpointName);
                }
                return endpoint;
            }
        }

        /// <inheritdoc />
        public virtual bool TryGetEndpointByAddress(Uri address, out IEndpoint endpoint)
        {
            var comparer = new EndpointAddressEqualityComparer();
            endpoint = _endpoints.Values.FirstOrDefault(e => comparer.Equals(e.Address, address));
            return endpoint != null;
        }

        /// <inheritdoc />
        public virtual bool Contains(EndpointName endpointName)
        {
            return _endpoints.ContainsKey(endpointName);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<EndpointName, IEndpoint>> GetEnumerator()
        {
            return _endpoints.GetEnumerator();
        }
    }
}
