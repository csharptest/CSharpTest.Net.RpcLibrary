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
using System.Runtime.Serialization;

namespace CSharpTest.Net.RpcLibrary.Interop
{
    internal class Ptr<T> : IDisposable
    {
        private readonly GCHandle _handle;

        public Ptr(T data)
        {
            _handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        }

        public T Data
        {
            get { return (T) _handle.Target; }
        }

        public IntPtr Handle
        {
            get { return _handle.AddrOfPinnedObject(); }
        }

        public void Dispose()
        {
            _handle.Free();
        }
    }

    internal class FunctionPtr<T> : IDisposable
        //whish I could: where T : Delegate
        where T : class, ICloneable, ISerializable
    {
        private T _delegate;
        public IntPtr Handle;

        public FunctionPtr(T data)
        {
            _delegate = data;
            Handle = Marshal.GetFunctionPointerForDelegate((Delegate) (object) data);
        }

        void IDisposable.Dispose()
        {
            _delegate = null;
            Handle = IntPtr.Zero;
        }
    }
}