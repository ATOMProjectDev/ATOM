using System;
using Grpc.Core;

namespace TollOperatorServer
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 50051;
            
            Server server = new Server
            {
                Services = { Atom.TollAuditService.BindService(new TollOperation_API()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Atom server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
