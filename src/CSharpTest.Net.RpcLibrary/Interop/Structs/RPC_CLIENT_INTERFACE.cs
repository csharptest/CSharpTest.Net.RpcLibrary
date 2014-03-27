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
    internal struct RPC_CLIENT_INTERFACE
    {
        public uint Length;
        public RPC_SYNTAX_IDENTIFIER InterfaceId;
        public RPC_SYNTAX_IDENTIFIER TransferSyntax;
        public IntPtr /*PRPC_DISPATCH_TABLE*/ DispatchTable;
        public uint RpcProtseqEndpointCount;
        public IntPtr /*PRPC_PROTSEQ_ENDPOINT*/ RpcProtseqEndpoint;
        public IntPtr Reserved;
        public IntPtr InterpreterInfo;
        public uint Flags;

        public static readonly Guid IID_SYNTAX = new Guid(0x8A885D04u, 0x1CEB, 0x11C9, 0x9F, 0xE8, 0x08, 0x00, 0x2B,
                                                          0x10,
                                                          0x48, 0x60);

        public RPC_CLIENT_INTERFACE(Guid iid)
        {
            Length = (uint) Marshal.SizeOf(typeof (RPC_CLIENT_INTERFACE));
            InterfaceId = new RPC_SYNTAX_IDENTIFIER() {SyntaxGUID = iid, SyntaxVersion = RPC_VERSION.INTERFACE_VERSION};
            TransferSyntax = new RPC_SYNTAX_IDENTIFIER()
                                 {SyntaxGUID = IID_SYNTAX, SyntaxVersion = RPC_VERSION.SYNTAX_VERSION};
            DispatchTable = IntPtr.Zero;
            RpcProtseqEndpointCount = 0u;
            RpcProtseqEndpoint = IntPtr.Zero;
            Reserved = IntPtr.Zero;
            InterpreterInfo = IntPtr.Zero;
            Flags = 0u;
        }
    }
}