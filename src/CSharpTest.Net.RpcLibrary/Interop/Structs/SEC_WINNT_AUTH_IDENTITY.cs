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
using System.Net;
using System.Runtime.InteropServices;

namespace CSharpTest.Net.RpcLibrary.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [System.Diagnostics.DebuggerDisplay("{Domain}\\{User}")]
    internal struct SEC_WINNT_AUTH_IDENTITY
    {
        public SEC_WINNT_AUTH_IDENTITY(NetworkCredential cred)
            : this(cred.Domain, cred.UserName, cred.Password)
        {
        }

        public SEC_WINNT_AUTH_IDENTITY(string domain, string user, string password)
        {
            User = user;
            UserLength = (uint) user.Length;
            Domain = domain;
            DomainLength = (uint) domain.Length;
            Password = password;
            PasswordLength = (uint) password.Length;
            Flags = SEC_WINNT_AUTH_IDENTITY_UNICODE;
        }

        //private const uint SEC_WINNT_AUTH_IDENTITY_ANSI = 0x1;
        private const uint SEC_WINNT_AUTH_IDENTITY_UNICODE = 0x2;

        private readonly String User;
        private readonly uint UserLength;
        private readonly String Domain;
        private readonly uint DomainLength;
        private readonly String Password;
        private readonly uint PasswordLength;
        private readonly uint Flags;
    }
}