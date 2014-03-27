#region Copyright 2014 by Roger Knapp, Licensed under the Apache License, Version 2.0
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
using CSharpTest.Net.RpcLibrary;

namespace ExampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // The client and server must agree on the interface id to use:
            var iid = new Guid("{1B617C4B-BF68-4B8C-AE2B-A77E6A3ECEC5}");
            
            // Create the server instance, adjust the defaults to your needs.
            using (var server = new RpcServerApi(iid, 100, ushort.MaxValue, allowAnonTcp: false))
            {
                try
                {
                    // Add an endpoint so the client can connect, this is local-host only:
                    server.AddProtocol(RpcProtseq.ncalrpc, "RpcExampleClientServer", 100);

                    // If you want to use TCP/IP uncomment the following, make sure your client authenticates or allowAnonTcp is true
                    // server.AddProtocol(RpcProtseq.ncacn_ip_tcp, @"8080", 25);

                    // Add the types of authentication we will accept
                    server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_GSS_NEGOTIATE);
                    server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_WINNT);
                    server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_NONE);

                    // Subscribe the code to handle requests on this event:
                    server.OnExecute +=
                        delegate(IRpcClientInfo client, byte[] bytes)
                            {
                                //Impersonate the caller:
                                using (client.Impersonate())
                                {
                                    var reqBody = Encoding.UTF8.GetString(bytes);
                                    Console.WriteLine("Received '{0}' from {1}", reqBody, client.ClientUser.Name);

                                    return Encoding.UTF8.GetBytes(
                                        String.Format(
                                            "Hello {0}, I received your message '{1}'.",
                                            client.ClientUser.Name,
                                            reqBody
                                            )
                                        );
                                }
                            };

                    // Start Listening 
                    server.StartListening();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }

                // Wait until you are done...
                Console.WriteLine("Press [Enter] to exit...");
                Console.ReadLine();
            }
        }
    }
}
