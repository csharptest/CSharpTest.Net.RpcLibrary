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
using System.Security.Principal;

namespace CSharpTest.Net.RpcLibrary
{
    /// <summary>
    /// An interface that provide contextual information about the client within an Rpc server call
    /// </summary>
    public interface IRpcClientInfo
    {
        /// <summary>
        /// Returns true if the caller is using LRPC
        /// </summary>
        bool IsClientLocal { get; }
        /// <summary>
        /// Returns a most random set of bytes, undocumented Win32 value.
        /// </summary>
        byte[] ClientAddress { get; }

        /// <summary>
        /// Defines the type of the procol being used in the communication, unavailable on Windows XP
        /// </summary>
        RpcProtoseqType ProtocolType { get; }
        /// <summary>
        /// Returns the packet protection level of the communications
        /// </summary>
        RpcProtectionLevel ProtectionLevel { get; }
        /// <summary>
        /// Returns the authentication level of the connection
        /// </summary>
        RpcAuthentication AuthenticationLevel { get; }
        /// <summary>
        /// Returns the ProcessId of the LRPC caller, may not be valid on all platforms
        /// </summary>
        IntPtr ClientPid { get; }
        /// <summary>
        /// Returns true if the caller has authenticated as a user
        /// </summary>
        bool IsAuthenticated { get; }
        /// <summary>
        /// Returns the client user name if authenticated, not available on WinXP
        /// </summary>
        string ClientPrincipalName { get; }
        /// <summary>
        /// Returns the identity of the client user or Anonymous if unauthenticated
        /// </summary>
        WindowsIdentity ClientUser { get; }
        /// <summary>
        /// Returns true if already impersonating the caller
        /// </summary>
        bool IsImpersonating { get; }
        /// <summary>
        /// Returns a disposable context that is used to impersonate the calling user
        /// </summary>
        IDisposable Impersonate();
    }
}