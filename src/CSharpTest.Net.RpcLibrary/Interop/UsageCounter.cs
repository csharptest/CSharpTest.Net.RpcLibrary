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
using System.Threading;

namespace CSharpTest.Net.RpcLibrary.Interop
{
    class UsageCounter
    {
        private const int MaxCount = int.MaxValue;
        private const int Timeout = 120000;

        readonly Mutex _lock;
        readonly Semaphore _count;

        /// <summary> Creates a composite name with the format and arguments specified </summary>
        public UsageCounter(string nameFormat, params object[] arguments)
        {
            string name = String.Format(nameFormat, arguments);
            _lock = new Mutex(false, name + ".Lock");
            _count = new Semaphore(MaxCount, MaxCount, name + ".Count");
        }

        /// <summary> Delegate fired inside lock if this is the first Increment() call on the name provided </summary>
        public void Increment<T>(Action<T> beginUsage, T arg)
        {
            if (!_lock.WaitOne(Timeout, false))
                throw new TimeoutException();
            try
            {
                if (!_count.WaitOne(Timeout, false))
                    throw new TimeoutException();

                if (!_count.WaitOne(Timeout, false))
                {
                    _count.Release();
                    throw new TimeoutException();
                }

                int counter = 1 + _count.Release();

                //if this is the first call
                if (beginUsage != null && counter == (MaxCount - 1))
                    beginUsage(arg);
            }
            finally
            {
                _lock.ReleaseMutex();
            }
        }

        /// <summary> Delegate fired inside lock if the Decrement() count reaches zero </summary>
        public void Decrement(ThreadStart endUsage)
        {
            if (!_lock.WaitOne(Timeout, false))
                throw new TimeoutException();
            try
            {
                int counter = 1 + _count.Release();

                //if this is the last decrement expected
                if (endUsage != null && counter == MaxCount)
                    endUsage();
            }
            finally
            {
                _lock.ReleaseMutex();
            }
        }
    }
}
