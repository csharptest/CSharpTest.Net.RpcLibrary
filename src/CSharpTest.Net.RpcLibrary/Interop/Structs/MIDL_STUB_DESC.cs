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
    internal delegate IntPtr LocalAlloc(uint size);

    internal delegate void LocalFree(IntPtr ptr);

    [StructLayout(LayoutKind.Sequential)]
    internal struct MIDL_STUB_DESC
    {
        public IntPtr /*RPC_CLIENT_INTERFACE*/ RpcInterfaceInformation;
        public IntPtr pfnAllocate;
        public IntPtr pfnFree;
        public IntPtr pAutoBindHandle;
        private IntPtr /*NDR_RUNDOWN*/ apfnNdrRundownRoutines;
        private IntPtr /*GENERIC_BINDING_ROUTINE_PAIR*/ aGenericBindingRoutinePairs;
        private IntPtr /*EXPR_EVAL*/ apfnExprEval;
        private IntPtr /*XMIT_ROUTINE_QUINTUPLE*/ aXmitQuintuple;
        public IntPtr pFormatTypes;
        public int fCheckBounds;
        /* Ndr library version. */
        public uint Version;
        private IntPtr /*MALLOC_FREE_STRUCT*/ pMallocFreeStruct;
        public int MIDLVersion;
        public IntPtr CommFaultOffsets;
        // New fields for version 3.0+
        private IntPtr /*USER_MARSHAL_ROUTINE_QUADRUPLE*/ aUserMarshalQuadruple;
        // Notify routines - added for NT5, MIDL 5.0
        private IntPtr /*NDR_NOTIFY_ROUTINE*/ NotifyRoutineTable;
        public IntPtr mFlags;
        // International support routines - added for 64bit post NT5
        private IntPtr /*NDR_CS_ROUTINES*/ CsRoutineTables;
        private IntPtr ProxyServerInfo;
        private IntPtr /*NDR_EXPR_DESC*/ pExprInfo;
        // Fields up to now present in win2000 release.

        public MIDL_STUB_DESC(RpcHandle handle, IntPtr interfaceInfo, Byte[] formatTypes, bool serverSide)
        {
            RpcInterfaceInformation = interfaceInfo;
            pfnAllocate = RpcApi.AllocPtr.Handle;
            pfnFree = RpcApi.FreePtr.Handle;
            pAutoBindHandle = serverSide ? IntPtr.Zero : handle.Pin(new IntPtr());
            apfnNdrRundownRoutines = new IntPtr();
            aGenericBindingRoutinePairs = new IntPtr();
            apfnExprEval = new IntPtr();
            aXmitQuintuple = new IntPtr();
            pFormatTypes = handle.Pin(formatTypes);
            fCheckBounds = 1;
            Version = 0x50002u;
            pMallocFreeStruct = new IntPtr();
            MIDLVersion = 0x70001f4;
            CommFaultOffsets = serverSide
                                   ? IntPtr.Zero
                                   : handle.Pin(new COMM_FAULT_OFFSETS() {CommOffset = -1, FaultOffset = -1});
            aUserMarshalQuadruple = new IntPtr();
            NotifyRoutineTable = new IntPtr();
            mFlags = new IntPtr(0x00000001);
            CsRoutineTables = new IntPtr();
            ProxyServerInfo = new IntPtr();
            pExprInfo = new IntPtr();
        }
    }
}