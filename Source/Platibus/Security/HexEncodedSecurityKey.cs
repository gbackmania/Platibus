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

#if NET452 || NET461
using System.IdentityModel.Tokens;
#endif
#if NETSTANDARD2_0
using Microsoft.IdentityModel.Tokens;
#endif

namespace Platibus.Security
{
    /// <inheritdoc />
    /// <summary>
    /// A <see cref="T:System.IdentityModel.Tokens.SecurityKey" /> implementation based on a bytes represented as hexadecimal
    /// degits
    /// </summary>
#if NET452 || NET461
    public class HexEncodedSecurityKey : InMemorySymmetricSecurityKey
#endif
#if NETSTANDARD2_0
    public class HexEncodedSecurityKey : SymmetricSecurityKey
#endif
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Security.HexEncodedSecurityKey" /> based on the specified
        /// <paramref name="hex" /> string representation
        /// </summary>
        /// <param name="hex">The hexadecimal string representation of the key bytes</param>
        public HexEncodedSecurityKey(string hex) : base(HexEncoding.GetBytes(hex))
        {
        }

        /// <summary>
        /// Implicitly casts a hexadecimal string to a <see cref="HexEncodedSecurityKey"/>
        /// </summary>
        /// <param name="hex">The hexadecimal encoded key bytes</param>
        /// <returns>Returns <c>null</c> if <paramref name="hex"/> is <c>null</c> or 
        /// whitespace; otherwise returns a new <see cref="HexEncodedSecurityKey"/> initialized
        /// with the decoded bytes from the specified <paramref name="hex"/> string</returns>
        /// <exception cref="System.FormatException">Thrown if <paramref name="hex"/> is not a
        /// valid hexadecimal string</exception>
        public static implicit operator HexEncodedSecurityKey(string hex)
        {
            return string.IsNullOrWhiteSpace(hex)
                ? null
                : new HexEncodedSecurityKey(hex);
        }
    }
}