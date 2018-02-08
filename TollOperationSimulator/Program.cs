using System;
using System.Threading;

namespace TollOperationSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            //while (true)
            {
                // TODO: REPORT BUG - ENUM VALUE WITH ZERO FIELD NUMBER DOES NOT PRINT WITH TOSTRING FUNCTION IN CONSOLE.WRITELINE: Console.WriteLine(VehicleInfo.ToString());
                foreach(var vehicle in VehicleInfoGenerator.GenerateVehicleInfo(10, 1000))
                {
                    Console.WriteLine(VehicleInfoGenerator.VehicleInfoToString(vehicle));
                }
                //Console.WriteLine(VehicleInfoGenerator.PrintVehicleInfo(VehicleInfoGenerator.GenerateVehicleInfo(1, 1000)[0]));//(VehicleInfoGenerator.GenerateVehicleInfo().ToString());
                //Thread.Sleep(4000);
            }
            Console.ReadKey();
        }
    }
}
