using System;
using System.Collections.Generic;
using System.Text;
using Atom;

namespace TollOperationSimulator
{
    public static class VehicleInfoGenerator
    {
        public static List<TollVehicleInfo> GenerateVehicleInfo(int count, int millisec)
        {
            //date date = DateConverter.Convert(2018, 1, 12, 22, 35, 20);
            List<TollVehicleInfo> vehicleInfo = new List<TollVehicleInfo>(count);
            for (int index = 0; index < count; index++)
            {
                vehicleInfo.Add(new TollVehicleInfo() { Timestamp = new date { Nanosecs = DateTime.Now.Ticks }, Vehicle = VehicleGenerator.GenerateVehicle() });
                System.Threading.Thread.Sleep(millisec);
            }
            return vehicleInfo;
        }

        public static string VehicleInfoToString(TollVehicleInfo data)
        {
            return string.Format("Transponder ID: {0}\n License Plate: {1}\n Vehicle Class: {2} \nTimestamp: {3}\n",
                data.Vehicle.TransponderID, data.Vehicle.LicensePlateNumber, data.Vehicle.VehicleClass, data.Timestamp);
        }
    }
}
