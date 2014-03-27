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
using NUnit.Framework;

namespace CSharpTest.Net.RpcLibrary.Test
{
    [TestFixture]
    public class TestClientApi
    {
        [TestFixtureSetUp]
        public void VerboseLog()
        { RpcServerApi.VerboseLogging = true; }

        [Test]
        public void TestPropertyProtocol()
        {
            using (RpcClientApi client = new RpcClientApi(Guid.NewGuid(), RpcProtseq.ncacn_ip_tcp, null, "123"))
                Assert.AreEqual(RpcProtseq.ncacn_ip_tcp, client.Protocol);
        }

        [Test]
        public void TestClientAbandon()
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

                {
                    RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncalrpc, null, "lrpctest");
                    client.AuthenticateAs(null, RpcClientApi.Self, RpcProtectionLevel.RPC_C_PROTECT_LEVEL_PKT_PRIVACY, RpcAuthentication.RPC_C_AUTHN_WINNT);
                    client.Execute(new byte[0]);
                    client = null;
                }

                GC.Collect(0, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();

                server.StopListening();
            }
        }

        [Test, ExpectedException(typeof(RpcException))]
        public void TestClientCannotConnect()
        {
            using (RpcClientApi client = new RpcClientApi(Guid.NewGuid(), RpcProtseq.ncalrpc, null, "lrpc-endpoint-doesnt-exist"))
                client.Execute(new byte[0]);
        }

        [Test, ExpectedException(typeof(RpcException), ExpectedMessage = "The requested operation is not supported")]
        public void TestExceptionAssertRpcError()
        { RpcException.Assert(RpcError.RPC_S_CANNOT_SUPPORT); }

        [Test, ExpectedException(typeof(RpcException), ExpectedMessage = "TEST_MESSAGE")]
        public void TestExceptionExplicitMessage()
        { throw new RpcException("TEST_MESSAGE"); }

        [Test, ExpectedException(typeof(RpcException), ExpectedMessage = "Unspecified rpc error")]
        public void TestExceptionDefaultMessage()
        { throw new RpcException(); }

    }
}
