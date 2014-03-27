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
    internal struct MIDL_SERVER_INFO
    {
        public IntPtr /* PMIDL_STUB_DESC */ pStubDesc;
        public IntPtr /* SERVER_ROUTINE* */ DispatchTable;
        public IntPtr /* PFORMAT_STRING */ ProcString;
        public IntPtr /* unsigned short* */ FmtStringOffset;
        private IntPtr /* STUB_THUNK * */ ThunkTable;
        private IntPtr /* PRPC_SYNTAX_IDENTIFIER */ pTransferSyntax;
        private IntPtr /* ULONG_PTR */ nCount;
        private IntPtr /* PMIDL_SYNTAX_INFO */ pSyntaxInfo;

        internal static Ptr<RPC_SERVER_INTERFACE> Create(RpcHandle handle, Guid iid, Byte[] formatTypes,
                                                         Byte[] formatProc,
                                                         RpcExecute fnExecute)
        {
            Ptr<MIDL_SERVER_INFO> pServer = handle.CreatePtr(new MIDL_SERVER_INFO());

            MIDL_SERVER_INFO temp = new MIDL_SERVER_INFO();
            return temp.Configure(handle, pServer, iid, formatTypes, formatProc, fnExecute);
        }

        private Ptr<RPC_SERVER_INTERFACE> Configure(RpcHandle handle, Ptr<MIDL_SERVER_INFO> me, Guid iid,
                                                    Byte[] formatTypes,
                                                    Byte[] formatProc, RpcExecute fnExecute)
        {
            Ptr<RPC_SERVER_INTERFACE> svrIface = handle.CreatePtr(new RPC_SERVER_INTERFACE(handle, me, iid));
            Ptr<MIDL_STUB_DESC> stub = handle.CreatePtr(new MIDL_STUB_DESC(handle, svrIface.Handle, formatTypes, true));
            pStubDesc = stub.Handle;

            IntPtr ptrFunction = handle.PinFunction(fnExecute);
            DispatchTable = handle.Pin(ptrFunction);

            ProcString = handle.Pin(formatProc);
            FmtStringOffset = handle.Pin(new int[1] {0});

            ThunkTable = IntPtr.Zero;
            pTransferSyntax = IntPtr.Zero;
            nCount = IntPtr.Zero;
            pSyntaxInfo = IntPtr.Zero;

            //Copy us back into the pinned address
            Marshal.StructureToPtr(this, me.Handle, false);
            return svrIface;
        }
    }

    internal delegate uint RpcExecute(
        IntPtr clientHandle, uint szInput, IntPtr input, out uint szOutput, out IntPtr output);
}