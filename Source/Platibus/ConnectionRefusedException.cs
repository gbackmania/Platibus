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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Platibus
{
    /// <summary>
    /// Exception thrown to indicate that a connection to a remote bus instance
    /// was refused
    /// </summary>
    [Serializable]
    public class ConnectionRefusedException : TransportException
    {
        /// <summary>
        /// Initializes a new <see cref="ConnectionRefusedException"/> with the
        /// specified <paramref name="host"/> and <paramref name="port"/>
        /// </summary>
        /// <param name="host">The host that refused the connection</param>
        /// <param name="port">The port on which the connection was refused</param>
        public ConnectionRefusedException(string host, int port)
            : base($"Connection refused to {host}:{port}")
        {
            Host = host;
            Port = port;
        }

        /// <summary>
        /// Initializes a new <see cref="ConnectionRefusedException"/> with the
        /// specified <paramref name="host"/>, <paramref name="port"/>, and nested
        /// exception
        /// </summary>
        /// <param name="host">The host that refused the connection</param>
        /// <param name="port">The port on which the connection was refused</param>
        /// <param name="innerException">The nested exception</param>
        public ConnectionRefusedException(string host, int port, Exception innerException)
            : base($"Connection refused to {host}:{port}", innerException)
        {
            Host = host;
            Port = port;
        }

        /// <summary>
        /// Initializes a serialized <see cref="ConnectionRefusedException"/> from a
        /// streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public ConnectionRefusedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Host = info.GetString("Host");
            Port = info.GetInt32("Port");
        }

        /// <summary>
        /// The host that refused the connection
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The port on which the connection was refused
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param><param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param><exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/></PermissionSet>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Host", Host);
            info.AddValue("Port", Port);
        }
    }
}