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
using System.Text;
using NUnit.Framework;

namespace CSharpTest.Net.RpcLibrary.Test
{
    [TestFixture]
    public class TestProtocols
    {
        string[] LocalNames = new string[] { null, "localhost", "127.0.0.1", "::1", Environment.MachineName };

        [TestFixtureSetUp]
        public void SetVerbose()
        { RpcServerApi.VerboseLogging = true; }

        [Test]
        public void TcpIpTest()
        {
            ReversePingTest(RpcProtseq.ncacn_ip_tcp, LocalNames, "18080", 
                RpcAuthentication.RPC_C_AUTHN_WINNT, RpcAuthentication.RPC_C_AUTHN_GSS_NEGOTIATE);
        }

        [Test]
        public void NamedPipeTest()
        {
            ReversePingTest(RpcProtseq.ncacn_np, LocalNames, @"\pipe\testpipename", 
                RpcAuthentication.RPC_C_AUTHN_NONE, RpcAuthentication.RPC_C_AUTHN_WINNT, RpcAuthentication.RPC_C_AUTHN_GSS_NEGOTIATE);
        }

        [Test]
        public void LocalRpcTest()
        {
            ReversePingTest(RpcProtseq.ncalrpc, new string[] { null }, @"testsomename", 
                RpcAuthentication.RPC_C_AUTHN_NONE, RpcAuthentication.RPC_C_AUTHN_WINNT, RpcAuthentication.RPC_C_AUTHN_GSS_NEGOTIATE);
        }

        /*
         *  Helper Methods
         */

        static void ReversePingTest(RpcProtseq protocol, string[] hostNames, string endpoint, params RpcAuthentication[] authTypes)
        {
            foreach (RpcAuthentication auth in authTypes)
                ReversePingTest(protocol, hostNames, endpoint, auth);
        }

        static void ReversePingTest(RpcProtseq protocol, string[] hostNames, string endpoint, RpcAuthentication auth)
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.OnExecute += 
                    delegate(IRpcClientInfo client, byte[] arg)
                    {
                        Array.Reverse(arg);
                        return arg;
                    };

                server.AddProtocol(protocol, endpoint, 5);
                server.AddAuthentication(auth);
                server.StartListening();

                byte[] input = Encoding.ASCII.GetBytes("abc");
                byte[] expect = Encoding.ASCII.GetBytes("cba");

                foreach (string hostName in hostNames)
                {
                    using (RpcClientApi client = new RpcClientApi(iid, protocol, hostName, endpoint))
                    {
                        client.AuthenticateAs(null, auth == RpcAuthentication.RPC_C_AUTHN_NONE
                                                      ? RpcClientApi.Anonymous
                                                      : RpcClientApi.Self, 
                                                  auth == RpcAuthentication.RPC_C_AUTHN_NONE
                                                      ? RpcProtectionLevel.RPC_C_PROTECT_LEVEL_NONE
                                                      : RpcProtectionLevel.RPC_C_PROTECT_LEVEL_PKT_PRIVACY,
                                                  auth);

                        Assert.AreEqual(expect, client.Execute(input));
                    }
                }
            }
        }
    }
}
