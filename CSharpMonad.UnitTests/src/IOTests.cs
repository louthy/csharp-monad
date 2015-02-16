﻿////////////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
// 
// Copyright (c) 2014 Paul Louth
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad;
using Xunit;
using System.Reflection;
using System.IO;
using Monad.Utility;

namespace Monad.UnitTests
{
    public class IOTests
    {
        [Fact]
		public void TestIOMonadLazyLoading()
        {
            var m = I.O(() =>
                System.IO.File.ReadAllBytes(Assembly.GetCallingAssembly().Location)
                );

			m ();
        }

        [Fact]
        public void TestIOMonadBinding()
        {
            string data = "Testing 123";

            var result = from tmpFileName   in GetTempFileName()
                         from _             in WriteFile(tmpFileName, data)
                         from dataFromFile  in ReadFile(tmpFileName)
                         from __            in DeleteFile(tmpFileName)
                         select dataFromFile;

            Assert.True(result.Invoke() == "Testing 123");
        }

        [Fact]
        public void TestIOMonadBindingFluent()
        {
            string data = "Testing 123";

            var result = GetTempFileName()
                            .Then( tmpFileName  => { WriteFile(tmpFileName, data)(); return tmpFileName; } )
                            .Then( tmpFileName  => { return new { tmpFileName, data = ReadFile(tmpFileName)() }; })
                            .Then( context      => { DeleteFile(context.tmpFileName)(); return context.data; } );

            Assert.True(result.Invoke() == "Testing 123");
        }

        private static IO<Unit> DeleteFile(string tmpFileName)
        {
            return () =>
                Unit.Return(
                    () => File.Delete(tmpFileName)
                );
        }

        private static IO<string> ReadFile(string tmpFileName)
        {
            return () => File.ReadAllText(tmpFileName);
        }

        private static IO<Unit> WriteFile(string tmpFileName, string data)
        {
            return () =>
                Unit.Return(
                    () => File.WriteAllText(tmpFileName, data)
                );
        }

        private static IO<string> GetTempFileName()
        {
            return () => Path.GetTempFileName();
        }
    }
}