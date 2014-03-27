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
using System.Diagnostics;
using NUnit.Framework;

namespace CSharpTest.Net.RpcLibrary.Test
{
    [TestFixture]
    public class TestPerformance
    {
        [TestFixtureSetUp]
        public void NoVerboseLogging()
        { RpcServerApi.VerboseLogging = false; }

        [Test]
        public void TestPerformanceWithLargePayloads()
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.AddProtocol(RpcProtseq.ncalrpc, "lrpctest", 5);
                server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_WINNT);
                server.StartListening();
                server.OnExecute +=
                    delegate(IRpcClientInfo client, byte[] arg)
                    { return arg; };

                using (RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncalrpc, null, "lrpctest"))
                {
                    client.AuthenticateAs(null, RpcClientApi.Self, RpcProtectionLevel.RPC_C_PROTECT_LEVEL_PKT_PRIVACY, RpcAuthentication.RPC_C_AUTHN_WINNT);
                    client.Execute(new byte[0]);

                    byte[] bytes = new byte[1 * 1024 * 1024]; //1mb in/out
                    new Random().NextBytes(bytes);

                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    for (int i = 0; i < 50; i++)
                        client.Execute(bytes);

                    timer.Stop();
                    Trace.WriteLine(timer.ElapsedMilliseconds.ToString(), "ncalrpc-large-timming");
                }
            }
        }

        [Test]
        public void TestPerformanceOnLocalRpc()
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.AddProtocol(RpcProtseq.ncalrpc, "lrpctest", 5);
                server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_WINNT);
                server.StartListening();
                server.OnExecute +=
                    delegate(IRpcClientInfo client, byte[] arg)
                    { return arg; };

                using (RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncalrpc, null, "lrpctest"))
                {
                    client.AuthenticateAs(null, RpcClientApi.Self, RpcProtectionLevel.RPC_C_PROTECT_LEVEL_PKT_PRIVACY, RpcAuthentication.RPC_C_AUTHN_WINNT);
                    client.Execute(new byte[0]);

                    byte[] bytes = new byte[512];
                    new Random().NextBytes(bytes);

                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    for (int i = 0; i < 10000; i++)
                        client.Execute(bytes);

                    timer.Stop();
                    Trace.WriteLine(timer.ElapsedMilliseconds.ToString(), "ncalrpc-timming");
                }
            }
        }

        [Test]
        public void TestPerformanceOnNamedPipe()
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.AddProtocol(RpcProtseq.ncacn_np, @"\pipe\testpipename", 5);
                server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_WINNT);
                server.StartListening();
                server.OnExecute +=
                    delegate(IRpcClientInfo client, byte[] arg)
                    { return arg; };

                using (RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncacn_np, null, @"\pipe\testpipename"))
                {
                    client.AuthenticateAs(null, RpcClientApi.Self, RpcProtectionLevel.RPC_C_PROTECT_LEVEL_PKT_PRIVACY, RpcAuthentication.RPC_C_AUTHN_WINNT);
                    client.Execute(new byte[0]);

                    byte[] bytes = new byte[512];
                    new Random().NextBytes(bytes);

                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    for (int i = 0; i < 5000; i++)
                        client.Execute(bytes);

                    timer.Stop();
                    Trace.WriteLine(timer.ElapsedMilliseconds.ToString(), "ncacn_np-timming");
                }
            }
        }

        [Test]
        public void TestPerformanceOnTcpip()
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.AddProtocol(RpcProtseq.ncacn_ip_tcp, @"18081", 5);
                server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_WINNT);
                server.StartListening();
                server.OnExecute +=
                    delegate(IRpcClientInfo client, byte[] arg)
                    { return arg; };

                using (RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncacn_ip_tcp, null, @"18081"))
                {
                    client.AuthenticateAs(null, RpcClientApi.Self, RpcProtectionLevel.RPC_C_PROTECT_LEVEL_PKT_PRIVACY, RpcAuthentication.RPC_C_AUTHN_WINNT);
                    client.Execute(new byte[0]);

                    byte[] bytes = new byte[512];
                    new Random().NextBytes(bytes);

                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    for (int i = 0; i < 4000; i++)
                        client.Execute(bytes);

                    timer.Stop();
                    Trace.WriteLine(timer.ElapsedMilliseconds.ToString(), "ncacn_ip_tcp-timming");
                }
            }
        }
    }
}
