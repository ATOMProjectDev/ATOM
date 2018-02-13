using System;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atom;

namespace TollOperatorServer
{
    class Program
    {
        static TollOperation_API tollAPI = new TollOperation_API();
        static async Task<List<TollVehicleInfo>> GetVehicle()
        {
            var result = await Task.Run(()=>TollOperationSimulator.VehicleInfoGenerator.GenerateVehicleInfo(18, 0));
            return result;
        }

        static void Main(string[] args)
        {
            const int Port = 50051;

            Server server = new Server
            {
                Services = { Atom.TollAuditService.BindService(tollAPI) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Atom server listening on port " + Port);
            while (Console.ReadLine() != "q")
            {
                var result = GetVehicle();
                tollAPI.Publish(result.Result[0]);
            }
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
