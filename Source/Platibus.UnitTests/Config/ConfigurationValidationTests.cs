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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Configuration;
using Xunit;
using Platibus.Config;

namespace Platibus.UnitTests.Config
{
    [Trait("Category", "UnitTests")]
    public class ConfigurationValidationTests
    {
        protected PlatibusConfiguration Configuration;

        [Fact]
        public void DefaultConfigurationIsValid()
        {
            GivenDefaultConfiguration();
            AssertDoesNotThrow(WhenValidating);
        }

        [Fact]
        public void MessageNamingServiceIsRequired()
        {
            GivenValidConfiguration();
            Configuration.MessageNamingService = null;
            Assert.Throws<ConfigurationErrorsException>(() => WhenValidating());
        }

        [Fact]
        public void SerializationServiceIsRequired()
        {
            GivenValidConfiguration();
            Configuration.SerializationService = null;
            Assert.Throws<ConfigurationErrorsException>(() => WhenValidating());
        }

        private void GivenDefaultConfiguration()
        {
            Configuration = new PlatibusConfiguration();
        }

        private void GivenValidConfiguration()
        {
            Configuration = new PlatibusConfiguration();
        }

        private void WhenValidating()
        {
            Configuration.Validate();
        }

        /// <summary>
        /// Helper method to improve readability
        /// </summary>
        /// <param name="action">The action that does not throw</param>
        private static void AssertDoesNotThrow(Action action)
        {
            action();
        }
    }
}
