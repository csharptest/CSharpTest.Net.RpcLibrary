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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSharpTest.Net.RpcLibrary.Interop.Structs
{
    internal enum RpcCallClientLocality : uint
    {
        Invalid = 0,
        Local = 1,
        Remote = 2,
        ClientUnknownLocality = 3
    }

    internal enum RpcCallType : uint
    {
        Invalid = 0,
        Normal = 1,
        Training = 2,
        Guaranteed = 3
    }

    internal enum RpcCallStatus : uint
    {
        Invalid = 0,
        InProgress = 1,
        Cancelled = 2,
        Disconnected = 3
    }

    internal enum RpcLocalAddressFormat : uint
    {
        Invalid = 0,
        IPv4 = 1,
        IPv6 = 2,
    }

    internal struct RPC_CALL_LOCAL_ADDRESS_V1
    {
        public uint Version;
        public IntPtr Buffer;
        public int BufferSize;
        public RpcLocalAddressFormat AddressFormat;
    }

    [Flags]
    internal enum RPC_CALL_ATTRIBUTES_FLAGS : int
    {
        RPC_QUERY_SERVER_PRINCIPAL_NAME = (0x02),
        RPC_QUERY_CLIENT_PRINCIPAL_NAME = (0x04),
        RPC_QUERY_CALL_LOCAL_ADDRESS = (0x08),
        RPC_QUERY_CLIENT_PID = (0x10),
        RPC_QUERY_IS_CLIENT_LOCAL = (0x20),
        RPC_QUERY_NO_AUTH_REQUIRED = (0x40),
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RPC_CALL_ATTRIBUTES_V2
    {
        public uint Version;
        public RPC_CALL_ATTRIBUTES_FLAGS Flags;
        public int ServerPrincipalNameBufferLength;
        public IntPtr ServerPrincipalName;
        public int ClientPrincipalNameBufferLength;
        public IntPtr ClientPrincipalName;
        public RpcProtectionLevel AuthenticationLevel;
        public RpcAuthentication AuthenticationService;
        public bool NullSession;
        public bool KernelMode;
        public RpcProtoseqType ProtocolSequence;
        public RpcCallClientLocality IsClientLocal;
        public IntPtr ClientPID;
        public RpcCallStatus CallStatus;
        public RpcCallType CallType;
        public IntPtr CallLocalAddress;
        public short OpNum;
        public Guid InterfaceUuid;
    }
}