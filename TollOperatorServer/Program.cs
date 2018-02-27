using System;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atom;

namespace TollOperatorServer
{
    class Program
    {
        static Dictionary<string, string> servers;
        static System.Threading.Timer _timer;
        static Random _random;
        static TollOperation_API tollAPI = new TollOperation_API();

        // generate vehicle
        static async Task<List<TollVehicleInfo>> GetVehicle()
        {
            return await Task.Run(()=>TollOperationSimulator.VehicleInfoGenerator.GenerateVehicleInfo(1)); // generate one vehicle on timer event fire
            
        }

        // timer callback that invokes method to generate vehicle
        private static void TimerCallback(object state)
        {
            try
            {
                var result = GetVehicle();
                tollAPI.Publish(result.Result[0]);
                Console.WriteLine("SERVER [TOLL1]: " + result.Result[0]);
            }
            finally
            {
                // randomly change timer interval for vehicle generation
                Task.Run(() => _timer.Change(_random.Next(100, 9000), System.Threading.Timeout.Infinite));
            }
        }

        static void Main(string[] args)
        {
            const int Port = 50052;
            _random = new Random();
            
            List<Server> servers = new List<Server>();

            Server server = new Server
            {
                Services = { Atom.TollAuditService.BindService(tollAPI) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();
            _timer = new System.Threading.Timer(TimerCallback, null, _random.Next(100, 9000), System.Threading.Timeout.Infinite);

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
