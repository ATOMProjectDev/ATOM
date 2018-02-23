using System;
using System.Collections.Generic;
using System.Text;
using Atom;

namespace TollOperationSimulator
{
     static class VehicleGenerator
    {
        public static Vehicle GenerateVehicle()
        {
            var vClass = GenerateVehicleClass();
            var uid = GenerateTransponderID();
            var license = GenerateLicensePlate();
            return new Vehicle { VehicleClass = vClass, TransponderID = uid, LicensePlateNumber = license };
        }

        static UID GenerateTransponderID()
        {
             return new UID { Uid = new Random().Next(1000000, 9999999).ToString() };
        }
        static string GenerateLicensePlate()
        {
            List<string> lgas = new List<string>();
            lgas.Add("ABC");
            lgas.Add("BWR");
            lgas.Add("ENU");
            lgas.Add("ABJ");
            lgas.Add("GWA");
            lgas.Add("BEN");
            lgas.Add("KWA");
            lgas.Add("NSR");
            lgas.Add("TAU");
            lgas.Add("AAA");
            lgas.Add("FG");
            // license plate format: ABC-123YZ
            return lgas[new Random().Next(0, lgas.Count)] + "-" + new Random().Next(1, 999) + ((char)new Random().Next(97, 122)).ToString().ToUpper() + 
                ((char)new Random().Next(97, 122)).ToString().ToUpper();
        }
        static VehicleClass GenerateVehicleClass()
        {
            Array values = Enum.GetValues(typeof(VehicleClass));
            Random random = new Random();
            return (VehicleClass)values.GetValue(random.Next(values.Length));
        }
    }
}
