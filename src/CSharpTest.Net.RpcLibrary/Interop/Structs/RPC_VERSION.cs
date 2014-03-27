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
using System.Runtime.InteropServices;

namespace CSharpTest.Net.RpcLibrary.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RPC_VERSION
    {
        public ushort MajorVersion;
        public ushort MinorVersion;


        public static readonly RPC_VERSION INTERFACE_VERSION = new RPC_VERSION() {MajorVersion = 1, MinorVersion = 0};
        public static readonly RPC_VERSION SYNTAX_VERSION = new RPC_VERSION() {MajorVersion = 2, MinorVersion = 0};
    }
}