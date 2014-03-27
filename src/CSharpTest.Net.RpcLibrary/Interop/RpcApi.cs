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
using CSharpTest.Net.RpcLibrary.Interop.Structs;

namespace CSharpTest.Net.RpcLibrary.Interop
{
    /// <summary>
    /// WinAPI imports for RPC
    /// </summary>
    internal static class RpcApi
    {
        #region MIDL_FORMAT_STRINGS

        internal static readonly bool Is64BitProcess;
        internal static readonly byte[] TYPE_FORMAT;
        internal static readonly byte[] FUNC_FORMAT;
        internal static readonly Ptr<Byte[]> FUNC_FORMAT_PTR;

        static RpcApi()
        {
            Is64BitProcess = (IntPtr.Size == 8);
            Log.Verbose("Is64BitProcess = {0}", Is64BitProcess);

            if (Is64BitProcess)
            {
                TYPE_FORMAT = new byte[]
                    {
                        0x00, 0x00, 0x1b, 0x00, 0x01, 0x00, 0x28, 0x00, 0x08, 0x00,
                        0x01, 0x00, 0x01, 0x5b, 0x11, 0x0c, 0x08, 0x5c, 0x11, 0x14,
                        0x02, 0x00, 0x12, 0x00, 0x02, 0x00, 0x1b, 0x00, 0x01, 0x00,
                        0x28, 0x54, 0x18, 0x00, 0x01, 0x00, 0x01, 0x5b, 0x00
                    };
                FUNC_FORMAT = new byte[]
                    {
                        0x00, 0x68, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x00,
                        0x32, 0x00, 0x00, 0x00, 0x08, 0x00, 0x24, 0x00, 0x47, 0x05,
                        0x0a, 0x07, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x48, 0x00, 0x08, 0x00, 0x08, 0x00, 0x0b, 0x00, 0x10, 0x00,
                        0x02, 0x00, 0x50, 0x21, 0x18, 0x00, 0x08, 0x00, 0x13, 0x20,
                        0x20, 0x00, 0x12, 0x00, 0x70, 0x00, 0x28, 0x00, 0x10, 0x00,
                        0x00
                    };
            }
            else
            {
                TYPE_FORMAT = new byte[]
                    {
                        0x00, 0x00, 0x1b, 0x00, 0x01, 0x00, 0x28, 0x00, 0x04, 0x00,
                        0x01, 0x00, 0x01, 0x5b, 0x11, 0x0c, 0x08, 0x5c, 0x11, 0x14,
                        0x02, 0x00, 0x12, 0x00, 0x02, 0x00, 0x1b, 0x00, 0x01, 0x00,
                        0x28, 0x54, 0x0c, 0x00, 0x01, 0x00, 0x01, 0x5b, 0x00
                    };
                FUNC_FORMAT = new byte[]
                    {
                        0x00, 0x68, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00,
                        0x32, 0x00, 0x00, 0x00, 0x08, 0x00, 0x24, 0x00, 0x47, 0x05,
                        0x08, 0x07, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x48, 0x00,
                        0x04, 0x00, 0x08, 0x00, 0x0b, 0x00, 0x08, 0x00, 0x02, 0x00,
                        0x50, 0x21, 0x0c, 0x00, 0x08, 0x00, 0x13, 0x20, 0x10, 0x00,
                        0x12, 0x00, 0x70, 0x00, 0x14, 0x00, 0x10, 0x00, 0x00
                    };
            }
            FUNC_FORMAT_PTR = new Ptr<byte[]>(FUNC_FORMAT);
        }

        #endregion

        #region Memory Utils

        [DllImport("Kernel32.dll", EntryPoint = "LocalFree", SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr LocalFree(IntPtr memHandle);

        internal static void Free(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Log.Verbose("LocalFree({0})", ptr);
                LocalFree(ptr);
            }
        }

        private const UInt32 LPTR = 0x0040;

        [DllImport("Kernel32.dll", EntryPoint = "LocalAlloc", SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr LocalAlloc(UInt32 flags, UInt32 nBytes);

        internal static IntPtr Alloc(uint size)
        {
            IntPtr ptr = LocalAlloc(LPTR, size);
            Log.Verbose("{0} = LocalAlloc({1})", ptr, size);
            return ptr;
        }

        [DllImport("Rpcrt4.dll", EntryPoint = "NdrServerCall2", CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern void NdrServerCall2(IntPtr ptr);

        internal delegate void ServerEntryPoint(IntPtr ptr);

        internal static FunctionPtr<ServerEntryPoint> ServerEntry = new FunctionPtr<ServerEntryPoint>(NdrServerCall2);

        internal static FunctionPtr<LocalAlloc> AllocPtr = new FunctionPtr<LocalAlloc>(Alloc);
        internal static FunctionPtr<LocalFree> FreePtr = new FunctionPtr<LocalFree>(Free);

        #endregion
    }
}