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
using System.Runtime.Serialization;

namespace CSharpTest.Net.RpcLibrary.Interop
{
    [System.Diagnostics.DebuggerDisplay("{Handle}")]
    internal abstract class RpcHandle : IDisposable
    {
        internal IntPtr Handle;
        private readonly List<IDisposable> _pinnedAddresses = new List<IDisposable>();

        internal IntPtr PinFunction<T>(T data)
            where T : class, ICloneable, ISerializable
        {
            FunctionPtr<T> instance = new FunctionPtr<T>(data);
            _pinnedAddresses.Add(instance);
            return instance.Handle;
        }

        internal IntPtr Pin<T>(T data)
        {
            return CreatePtr(data).Handle;
        }

        internal bool GetPtr<T>(out T value)
        {
            foreach (object o in _pinnedAddresses)
            {
                if (o is T)
                {
                    value = (T) o;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        internal Ptr<T> CreatePtr<T>(T data)
        {
            Ptr<T> ptr = new Ptr<T>(data);
            _pinnedAddresses.Add(ptr);
            return ptr;
        }

        ~RpcHandle()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            try
            {
                Log.Verbose("RpcHandle.Dispose on {0}", Handle);

                if (Handle != IntPtr.Zero)
                {
                    DisposeHandle(ref Handle);
                }

                for (int i = _pinnedAddresses.Count - 1; i >= 0; i--)
                {
                    _pinnedAddresses[i].Dispose();
                }
                _pinnedAddresses.Clear();
            }
            finally
            {
                Handle = IntPtr.Zero;
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        protected abstract void DisposeHandle(ref IntPtr handle);
    }
}