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
using System.Threading;
using CSharpTest.Net.RpcLibrary.Interop;
using CSharpTest.Net.RpcLibrary.Interop.Structs;

namespace CSharpTest.Net.RpcLibrary
{
    /// <summary>
    /// Provides server-side services for RPC
    /// </summary>
    public class RpcServerApi : IDisposable
    {
        /// <summary> The max limit of in-flight calls </summary>
        public const int MAX_CALL_LIMIT = 255;
        /// <summary> Use the default request limits </summary>
        public const int DEF_REQ_LIMIT = -1;

        private static readonly UsageCounter _listenerCount = new UsageCounter("RpcApi.Listener.{0}", System.Diagnostics.Process.GetCurrentProcess().Id);
        private bool _isListening;
        private int _maxCalls;

        /// <summary> The interface Id the service is using </summary>
        public readonly Guid IID;
        private readonly RpcHandle _handle;
        private RpcExecuteHandler _handler;

        /// <summary>
        /// Enables verbose logging of the RPC calls to the Trace window
        /// </summary>
        public static bool VerboseLogging
        {
            get { return Log.VerboseEnabled; }
            set { Log.VerboseEnabled = value; }
        }
        /// <summary>
        /// Constructs an RPC server for the given interface guid, the guid is used to identify multiple rpc
        /// servers/services within a single process.
        /// </summary>
        public RpcServerApi(Guid iid)
            : this(iid, MAX_CALL_LIMIT, DEF_REQ_LIMIT, false)
        {
        }
        /// <summary>
        /// Constructs an RPC server for the given interface guid, the guid is used to identify multiple rpc
        /// servers/services within a single process.
        /// </summary>
        public RpcServerApi(Guid iid, int maxCalls, int maxRequestBytes, bool allowAnonTcp)
        {
            IID = iid;
            _maxCalls = maxCalls;
            _handle = new RpcServerHandle();

            // Guid.Empty to avoid registration of any interface allowing access to AddProtocol/AddAuthentication
            // without creating a channel
            if (!Guid.Empty.Equals(iid))
                ServerRegisterInterface(_handle, IID, RpcEntryPoint, maxCalls, maxRequestBytes, allowAnonTcp);
        }

        /// <summary>
        /// Disposes of the server and stops listening if the server is currently listening
        /// </summary>
        public void Dispose()
        {
            _handler = null;
            StopListening();
            _handle.Dispose();
        }
        /// <summary>
        /// Used to ensure that the server is listening with a specific protocol type.  Once invoked this
        /// can not be undone, and all RPC servers within the process will be available on that protocol
        /// </summary>
        public bool AddProtocol(RpcProtseq protocol, string endpoint, int maxCalls)
        {
            _maxCalls = Math.Max(_maxCalls, maxCalls);
            return ServerUseProtseqEp(protocol, maxCalls, endpoint);
        }
        /// <summary>
        /// Adds a type of authentication sequence that will be allowed for RPC connections to this process.
        /// </summary>
        public bool AddAuthentication(RpcAuthentication type)
        {
            return AddAuthentication(type, null);
        }
        /// <summary>
        /// Adds a type of authentication sequence that will be allowed for RPC connections to this process.
        /// </summary>
        public bool AddAuthentication(RpcAuthentication type, string serverPrincipalName)
        {
            return ServerRegisterAuthInfo(type, serverPrincipalName);
        }
        /// <summary>
        /// Starts the RPC listener for this instance, if this is the first RPC server instance the process
        /// starts listening on the registered protocols.
        /// </summary>
        public void StartListening()
        {
            if (_isListening)
                return;

            _listenerCount.Increment(ServerListen, _maxCalls);
            _isListening = true;
        }
        /// <summary>
        /// Stops listening for this instance, if this is the last instance to stop listening the process
        /// stops listening on all registered protocols.
        /// </summary>
        public void StopListening()
        {
            if (!_isListening)
                return;

            _isListening = false;
            _listenerCount.Decrement(ServerStopListening);
        }

        private uint RpcEntryPoint(IntPtr clientHandle, uint szInput, IntPtr input, out uint szOutput, out IntPtr output)
        {
            output = IntPtr.Zero;
            szOutput = 0;

            try
            {
                byte[] bytesIn = new byte[szInput];
                Marshal.Copy(input, bytesIn, 0, bytesIn.Length);

                byte[] bytesOut;
                using (RpcClientInfo client = new RpcClientInfo(clientHandle))
                {
                    bytesOut = Execute(client, bytesIn);
                }
                if (bytesOut == null)
                {
                    return (uint) RpcError.RPC_S_NOT_LISTENING;
                }

                szOutput = (uint) bytesOut.Length;
                output = RpcApi.Alloc(szOutput);
                Marshal.Copy(bytesOut, 0, output, bytesOut.Length);

                return (uint) RpcError.RPC_S_OK;
            }
            catch (Exception ex)
            {
                RpcApi.Free(output);
                output = IntPtr.Zero;
                szOutput = 0;

                Log.Error(ex);
                return (uint) RpcError.RPC_E_FAIL;
            }
        }
        /// <summary>
        /// Can be over-ridden in a derived class to handle the incomming RPC request, or you can
        /// subscribe to the OnExecute event.
        /// </summary>
        public virtual byte[] Execute(IRpcClientInfo client, byte[] input)
        {
            RpcExecuteHandler proc = _handler;
            if (proc != null)
            {
                return proc(client, input);
            }
            return null;
        }
        /// <summary>
        /// Allows a single subscription to this event to handle incomming requests rather than 
        /// deriving from and overriding the Execute call.
        /// </summary>
        public event RpcExecuteHandler OnExecute
        {
            add
            {
                lock (this)
                {
                    Check.Assert<InvalidOperationException>(_handler == null, "The interface id is already registered.");
                    _handler = value;
                }
            }
            remove
            {
                lock (this)
                {
                    Check.NotNull(value);
                    if (_handler != null)
                        Check.Assert<InvalidOperationException>(
                            Object.ReferenceEquals(_handler.Target, value.Target)
                            && Object.ReferenceEquals(_handler.Method, value.Method)
                            );
                    _handler = null;
                }
            }
        }
        /// <summary>
        /// The delegate format for the OnExecute event
        /// </summary>
        public delegate byte[] RpcExecuteHandler(IRpcClientInfo client, byte[] input);

        /* ********************************************************************
         * WinAPI INTEROP
         * *******************************************************************/

        private class RpcServerHandle : RpcHandle
        {
            protected override void DisposeHandle(ref IntPtr handle)
            {
                if (handle != IntPtr.Zero)
                {
                    RpcServerUnregisterIf(handle, IntPtr.Zero, 1);
                    handle = IntPtr.Zero;
                }
            }
        }

        [DllImport("Rpcrt4.dll", EntryPoint = "RpcServerUnregisterIf", CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern RpcError RpcServerUnregisterIf(IntPtr IfSpec, IntPtr MgrTypeUuid, uint WaitForCallsToComplete);

        #region RpcServerXXXX routines

        [DllImport("Rpcrt4.dll", EntryPoint = "RpcServerUseProtseqEpW", CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern RpcError RpcServerUseProtseqEp(String Protseq, int MaxCalls, String Endpoint, IntPtr SecurityDescriptor);

        private static bool ServerUseProtseqEp(RpcProtseq protocol, int maxCalls, String endpoint)
        {
            Log.Verbose("ServerUseProtseqEp({0})", protocol);
            RpcError err = RpcServerUseProtseqEp(protocol.ToString(), maxCalls, endpoint, IntPtr.Zero);
            if (err != RpcError.RPC_S_DUPLICATE_ENDPOINT)
                RpcException.Assert(err);

            return err == RpcError.RPC_S_OK;
        }

        delegate int RPC_IF_CALLBACK_FN(IntPtr Interface, IntPtr Context);
        private static readonly FunctionPtr<RPC_IF_CALLBACK_FN> hAuthCall = new FunctionPtr<RPC_IF_CALLBACK_FN>(AuthCall);
        static int AuthCall(IntPtr Interface, IntPtr Context) 
        {
            return (int)RpcError.RPC_S_OK; 
        }

        [DllImport("Rpcrt4.dll", EntryPoint = "RpcServerRegisterIf2", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern RpcError RpcServerRegisterIf2(IntPtr IfSpec, IntPtr MgrTypeUuid, IntPtr MgrEpv, int Flags, int MaxCalls, int MaxRpcSize, IntPtr hProc);

        [DllImport("Rpcrt4.dll", EntryPoint = "RpcServerRegisterIf", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern RpcError RpcServerRegisterIf(IntPtr IfSpec, IntPtr MgrTypeUuid, IntPtr MgrEpv);

        private static void ServerRegisterInterface(RpcHandle handle, Guid iid, RpcExecute fnExec, int maxCalls, int maxRequestBytes, bool allowAnonTcp)
        {
            const int RPC_IF_ALLOW_CALLBACKS_WITH_NO_AUTH = 0x0010;
            int flags = 0;
            IntPtr fnAuth = IntPtr.Zero;
            if (allowAnonTcp)
            {
                flags = RPC_IF_ALLOW_CALLBACKS_WITH_NO_AUTH;
                fnAuth = hAuthCall.Handle;
            }

            Ptr<RPC_SERVER_INTERFACE> sIf = MIDL_SERVER_INFO.Create(handle, iid, RpcApi.TYPE_FORMAT, RpcApi.FUNC_FORMAT, fnExec);

            if (!allowAnonTcp && maxRequestBytes < 0)
                RpcException.Assert(RpcServerRegisterIf(sIf.Handle, IntPtr.Zero, IntPtr.Zero));
            else
                RpcException.Assert(RpcServerRegisterIf2(sIf.Handle, IntPtr.Zero, IntPtr.Zero, flags, 
                    maxCalls <= 0 ? MAX_CALL_LIMIT : maxCalls, 
                    maxRequestBytes <= 0 ? 80 * 1024 : maxRequestBytes, fnAuth));

            handle.Handle = sIf.Handle;
        }

        [DllImport("Rpcrt4.dll", EntryPoint = "RpcServerRegisterAuthInfoW",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern RpcError RpcServerRegisterAuthInfo(String ServerPrincName, uint AuthnSvc, IntPtr GetKeyFn,
                                                                 IntPtr Arg);

        private static bool ServerRegisterAuthInfo(RpcAuthentication auth, string serverPrincName)
        {
            Log.Verbose("ServerRegisterAuthInfo({0})", auth);
            RpcError response = RpcServerRegisterAuthInfo(serverPrincName, (uint) auth, IntPtr.Zero, IntPtr.Zero);
            if (response != RpcError.RPC_S_OK)
            {
                Log.Warning("ServerRegisterAuthInfo - unable to register authentication type {0}", auth);
                return false;
            }
            return true;
        }

        #endregion

        #region RpcServerListen & StopListening

        [DllImport("Rpcrt4.dll", EntryPoint = "RpcServerListen", CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern RpcError RpcServerListen(uint MinimumCallThreads, int MaxCalls, uint DontWait);

        private static void ServerListen(int maxCalls)
        {
            Log.Verbose("Begin Server Listening");
            RpcError result = RpcServerListen(1, maxCalls, 1);
            if (result == RpcError.RPC_S_ALREADY_LISTENING)
            {
                result = RpcError.RPC_S_OK;
            }
            RpcException.Assert(result);
            Log.Verbose("Server Ready");
        }

        [DllImport("Rpcrt4.dll", EntryPoint = "RpcMgmtStopServerListening",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern RpcError RpcMgmtStopServerListening(IntPtr ignore);

        [DllImport("Rpcrt4.dll", EntryPoint = "RpcMgmtWaitServerListen", CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern RpcError RpcMgmtWaitServerListen();

        private static void ServerStopListening()
        {
            Log.Verbose("Stop Server Listening");
            RpcError result = RpcMgmtStopServerListening(IntPtr.Zero);
            if (result != RpcError.RPC_S_OK)
            {
                Log.Warning("RpcMgmtStopServerListening result = {0}", result);
            }
            result = RpcMgmtWaitServerListen();
            if (result != RpcError.RPC_S_OK)
            {
                Log.Warning("RpcMgmtWaitServerListen result = {0}", result);
            }
        }

        #endregion
    }
}