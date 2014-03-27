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

namespace ExampleClient
{
    class Program
    {
        private static void Main(string[] args)
        {
            // The client and server must agree on the interface id to use:
            var iid = new Guid("{1B617C4B-BF68-4B8C-AE2B-A77E6A3ECEC5}");

            bool attempt = true;
            while (attempt)
            {
                attempt = false;
                // Open the connection based on the endpoint information and interface IID
                using (var client = new RpcClientApi(iid, RpcProtseq.ncalrpc, null, "RpcExampleClientServer"))
                    //using (var client = new RpcClientApi(iid, RpcProtseq.ncacn_ip_tcp, null, @"18081"))
                {
                    // Provide authentication information (not nessessary for LRPC)
                    client.AuthenticateAs(RpcClientApi.Self);

                    // Send the request and get a response
                    try
                    {
                        var response = client.Execute(Encoding.UTF8.GetBytes(args.Length == 0 ? "Greetings" : args[0]));
                        Console.WriteLine("Server response: {0}", Encoding.UTF8.GetString(response));
                    }
                    catch (RpcException rx)
                    {
                        if (rx.RpcError == RpcError.RPC_S_SERVER_UNAVAILABLE || rx.RpcError == RpcError.RPC_S_SERVER_TOO_BUSY)
                        {
                            //Use a wait handle if your on the same box...
                            Console.Error.WriteLine("Waiting for server...");
                            System.Threading.Thread.Sleep(1000);
                            attempt = true;
                        }
                        else
                            Console.Error.WriteLine(rx);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                }
            }
            // done...
            Console.WriteLine("Press [Enter] to exit...");
            Console.ReadLine();
        }
    }
}
