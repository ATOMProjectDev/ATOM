using System;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atom;

namespace TollOperatorServer
{
    class Program
    {
        static System.Threading.Timer _timer;
        static Random _random;
        static TollOperation_API tollAPI = new TollOperation_API();

        // generate vehicle
        static async Task<List<TollVehicleInfo>> GetVehicle()
        {
            var result = await Task.Run(()=>TollOperationSimulator.VehicleInfoGenerator.GenerateVehicleInfo(1, 0));
            return result;
        }

        // timer callback that invokes method to generate vehicle
        private static void TimerCallback(object state)
        {
            try
            {
                var result = GetVehicle();
                tollAPI.Publish(result.Result[0]);
            }
            finally
            {
                // randomly change timer interval for vehicle generation
                _timer.Change(_random.Next(1000, 10000), System.Threading.Timeout.Infinite);
            }
        }

        static void Main(string[] args)
        {
            const int Port = 50051;
            _random = new Random();
            _timer = new System.Threading.Timer(TimerCallback, null, _random.Next(1000, 10000), System.Threading.Timeout.Infinite);

            Server server = new Server
            {
                Services = { Atom.TollAuditService.BindService(tollAPI) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Atom server listening on port " + Port);
            while (Console.ReadLine() != "exit") ;
            //{
            //    var result = GetVehicle();
            //    tollAPI.Publish(result.Result[0]);
            //}
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
