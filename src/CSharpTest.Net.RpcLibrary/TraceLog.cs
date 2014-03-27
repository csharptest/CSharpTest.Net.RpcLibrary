#region Copyright 2010-2014 by Roger Knapp, Licensed under the Apache License, Version 2.0
/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion
using System;
using System.Diagnostics;

namespace CSharpTest.Net.RpcLibrary
{
    internal static class Log
    {
        private static readonly string Category = "RpcInterop";
        internal static bool VerboseEnabled = false;

        [Conditional("DEBUG")]
        public static void Verbose(string message)
        {
            if (VerboseEnabled)
            {
                Trace.WriteLine(message, Category);
            }
        }

        [Conditional("DEBUG")]
        public static void Verbose(string message, params object[] arguments)
        {
            if (VerboseEnabled)
            {
                try
                {
                    Verbose(String.Format(message, arguments));
                }
                catch
                {
                    Verbose(message);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            Trace.WriteLine(message, Category);
        }

        [Conditional("DEBUG")]
        public static void Warning(string message, params object[] arguments)
        {
            try
            {
                Warning(String.Format(message, arguments));
            }
            catch
            {
                Warning(message);
            }
        }

        [Conditional("DEBUG")]
        public static void Error(Exception error)
        {
            Error("{0}", error);
        }

        [Conditional("DEBUG")]
        public static void Error(string message, params object[] arguments)
        {
            try
            {
                Error(String.Format(message, arguments));
            }
            catch
            {
                Error(message);
            }
        }
    }
}