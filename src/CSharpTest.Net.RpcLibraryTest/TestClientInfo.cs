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
    public class TestClientInfo
    {
        [TestFixtureSetUp]
        public void SetVerbose()
        { RpcServerApi.VerboseLogging = true; }

        [Test]
        public void TestClientOnLocalRpc()
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.AddProtocol(RpcProtseq.ncalrpc, "lrpctest", 5);
                server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_WINNT);
                server.StartListening();
                server.OnExecute +=
                    delegate(IRpcClientInfo client, byte[] arg)
                        {
                            Assert.AreEqual(0, arg.Length);
                            Assert.AreEqual(RpcAuthentication.RPC_C_AUTHN_WINNT, client.AuthenticationLevel);
                            Assert.AreEqual(RpcProtectionLevel.RPC_C_PROTECT_LEVEL_PKT_PRIVACY, client.ProtectionLevel);
                            Assert.AreEqual(RpcProtoseqType.LRPC, client.ProtocolType);
                            Assert.AreEqual(new byte[0], client.ClientAddress);
                            Assert.AreEqual(System.Diagnostics.Process.GetCurrentProcess().Id, client.ClientPid.ToInt32());
                            Assert.AreEqual(System.Security.Principal.WindowsIdentity.GetCurrent().Name, client.ClientPrincipalName);
                            Assert.AreEqual(System.Security.Principal.WindowsIdentity.GetCurrent().Name, client.ClientUser.Name);
                            Assert.AreEqual(true, client.IsClientLocal);
                            Assert.AreEqual(true, client.IsAuthenticated);
                            Assert.AreEqual(false, client.IsImpersonating);
                            using(client.Impersonate())
                                Assert.AreEqual(true, client.IsImpersonating);
                            Assert.AreEqual(false, client.IsImpersonating);
                            return arg;
                        };

                using (RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncalrpc, null, "lrpctest"))
                {
                    client.AuthenticateAs(RpcClientApi.Self);
                    client.Execute(new byte[0]);
                }
            }
        }

        [Test]
        public void TestClientOnNamedPipe()
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.AddProtocol(RpcProtseq.ncacn_np, @"\pipe\testpipename", 5);
                server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_WINNT);
                server.StartListening();
                server.OnExecute +=
                    delegate(IRpcClientInfo client, byte[] arg)
                    {
                        Assert.AreEqual(0, arg.Length);
                        Assert.AreEqual(RpcAuthentication.RPC_C_AUTHN_WINNT, client.AuthenticationLevel);
                        Assert.AreEqual(RpcProtectionLevel.RPC_C_PROTECT_LEVEL_PKT_PRIVACY, client.ProtectionLevel);
                        Assert.AreEqual(RpcProtoseqType.NMP, client.ProtocolType);
                        Assert.AreEqual(new byte[0], client.ClientAddress);
                        Assert.AreEqual(0, client.ClientPid.ToInt32());
                        Assert.AreEqual(String.Empty, client.ClientPrincipalName);
                        Assert.AreEqual(System.Security.Principal.WindowsIdentity.GetCurrent().Name, client.ClientUser.Name);
                        Assert.AreEqual(true, client.IsClientLocal);
                        Assert.AreEqual(true, client.IsAuthenticated);
                        Assert.AreEqual(false, client.IsImpersonating);
                        using (client.Impersonate())
                            Assert.AreEqual(true, client.IsImpersonating);
                        Assert.AreEqual(false, client.IsImpersonating);
                        return arg;
                    };

                using (RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncacn_np, null, @"\pipe\testpipename"))
                {
                    client.AuthenticateAs(RpcClientApi.Self);
                    client.Execute(new byte[0]);
                }
            }
        }

        [Test]
        public void TestClientOnAnonymousPipe()
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.AddProtocol(RpcProtseq.ncacn_np, @"\pipe\testpipename", 5);
                server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_NONE);
                server.StartListening();
                server.OnExecute +=
                    delegate(IRpcClientInfo client, byte[] arg)
                    {
                        Assert.AreEqual(0, arg.Length);
                        Assert.AreEqual(RpcAuthentication.RPC_C_AUTHN_NONE, client.AuthenticationLevel);
                        Assert.AreEqual(RpcProtectionLevel.RPC_C_PROTECT_LEVEL_NONE, client.ProtectionLevel);
                        Assert.AreEqual(RpcProtoseqType.NMP, client.ProtocolType);
                        Assert.AreEqual(new byte[0], client.ClientAddress);
                        Assert.AreEqual(0, client.ClientPid.ToInt32());
                        Assert.AreEqual(null, client.ClientPrincipalName);
                        Assert.AreEqual(System.Security.Principal.WindowsIdentity.GetAnonymous().Name, client.ClientUser.Name);
                        Assert.AreEqual(true, client.IsClientLocal);
                        Assert.AreEqual(false, client.IsAuthenticated);
                        Assert.AreEqual(false, client.IsImpersonating);

                        bool failed = false;
                        try { client.Impersonate().Dispose(); }
                        catch (UnauthorizedAccessException) { failed = true; }
                        Assert.AreEqual(true, failed);
                        return arg;
                    };

                using (RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncacn_np, null, @"\pipe\testpipename"))
                {
                    client.AuthenticateAs(RpcClientApi.Anonymous);
                    client.Execute(new byte[0]);
                }
            }
        }

        [Test]
        public void TestClientOnTcpip()
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.AddProtocol(RpcProtseq.ncacn_ip_tcp, @"18081", 5);
                server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_WINNT);
                server.StartListening();
                server.OnExecute +=
                    delegate(IRpcClientInfo client, byte[] arg)
                    {
                        Assert.AreEqual(0, arg.Length);
                        Assert.AreEqual(RpcAuthentication.RPC_C_AUTHN_WINNT, client.AuthenticationLevel);
                        Assert.AreEqual(RpcProtectionLevel.RPC_C_PROTECT_LEVEL_PKT_PRIVACY, client.ProtectionLevel);
                        Assert.AreEqual(RpcProtoseqType.TCP, client.ProtocolType);
                        Assert.AreEqual(16, client.ClientAddress.Length);
                        Assert.AreEqual(0, client.ClientPid.ToInt32());
                        Assert.AreEqual(String.Empty, client.ClientPrincipalName);
                        Assert.AreEqual(System.Security.Principal.WindowsIdentity.GetCurrent().Name, client.ClientUser.Name);
                        Assert.AreEqual(false, client.IsClientLocal);
                        Assert.AreEqual(true, client.IsAuthenticated);
                        Assert.AreEqual(false, client.IsImpersonating);
                        using (client.Impersonate())
                            Assert.AreEqual(true, client.IsImpersonating);
                        Assert.AreEqual(false, client.IsImpersonating);
                        return arg;
                    };

                using (RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncacn_ip_tcp, null, @"18081"))
                {
                    client.AuthenticateAs(RpcClientApi.Self);
                    client.Execute(new byte[0]);
                }
            }
        }

        [Test]
        public void TestNestedClientImpersonate()
        {
            Guid iid = Guid.NewGuid();
            using (RpcServerApi server = new RpcServerApi(iid))
            {
                server.AddProtocol(RpcProtseq.ncacn_np, @"\pipe\testpipename", 5);
                server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_WINNT);
                server.StartListening();
                server.OnExecute +=
                    delegate(IRpcClientInfo client, byte[] arg)
                    {
                        Assert.AreEqual(false, client.IsImpersonating);
                        using (client.Impersonate())
                        {
                            Assert.AreEqual(true, client.IsImpersonating);
                            using (client.Impersonate())
                                Assert.AreEqual(true, client.IsImpersonating); 
                            //does not dispose, we are still impersonating
                            Assert.AreEqual(true, client.IsImpersonating);
                        }
                        Assert.AreEqual(false, client.IsImpersonating);
                        return arg;
                    };

                using (RpcClientApi client = new RpcClientApi(iid, RpcProtseq.ncacn_np, null, @"\pipe\testpipename"))
                {
                    client.AuthenticateAs(RpcClientApi.Self);
                    client.Execute(new byte[0]);
                }
            }
        }
    }
}
