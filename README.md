CSharpTest.Net.RpcLibrary
=======================

CSharpTest.Net.RpcLibrary (moved from https://code.google.com/p/csharptest-net/)

## Change Log ##

2014-03-26	Initial clone and extraction from existing library.

## Online Help ##

See the example client/server application in source...



## Quick start ##

### Example server application ###
```
var iid = new Guid("{1B617C4B-BF68-4B8C-AE2B-A77E6A3ECEC5}");
using (var server = new RpcServerApi(iid, 100, ushort.MaxValue, allowAnonTcp: false))
{
    // Add an endpoint so the client can connect, this is local-host only:
    server.AddProtocol(RpcProtseq.ncalrpc, "RpcExampleClientServer", 100);
    // Add the types of authentication we will accept
    server.AddAuthentication(RpcAuthentication.RPC_C_AUTHN_GSS_NEGOTIATE
    // Subscribe the code to handle requests on this event:
    server.OnExecute +=
        delegate(IRpcClientInfo client, byte[] bytes)
            { return new byte[0]; };
    // Start Listening 
    server.StartListening();
	// Wait...
	Console.ReadLine();
}
```

### Example client application ###
```
var iid = new Guid("{1B617C4B-BF68-4B8C-AE2B-A77E6A3ECEC5}");
using (var client = new RpcClientApi(iid, RpcProtseq.ncalrpc, null, "RpcExampleClientServer"))
    //using (var client = new RpcClientApi(iid, RpcProtseq.ncacn_ip_tcp, null, @"18081"))
{
    // Provide authentication information (not nessessary for LRPC)
    client.AuthenticateAs(RpcClientApi.Self);
    // Send the request and get a response
    byte[] response = client.Execute(new byte[0]);
}
```

### Interface Identifiers in RPC ###

Every Win32 RPC server is identified by an Interface GUID (IID).  A process may have only one instance listening on any give IID at a time.  

Unlike WCF and other connection-oriented channels, RPC keeps the connection apart from the objects avaialable.  This means that a single object instance can listen on multiple endpoints, including multiple different protocols.  It's important to understand that the IID is the 'object selector' in RPC not the endpoint you are listening on.  For instance, i