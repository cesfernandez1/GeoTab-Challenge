using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;

namespace GeoTab_Challenge
{
    class Program
    {
        private const string databaseName = "demo_cesarfernandez";
        private const string userEmail = "cesarfernandez@geotab-challenge.com";
        private const string password = "DN7Tz2NVc9qZ6%z";
        private const string server = "mypreview.geotab.com";
        private const string folder = @"C:\Data";


        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var api = new API(userEmail, password, null, databaseName, server);

            Console.WriteLine("Authenticating.....");

            try
            {
                await api.AuthenticateAsync();
            }
            catch (InvalidUserException)
            {
                // Here you can display the error and prompt for user to re-enter credentials
                Console.WriteLine(" User name or password incorrect");
                return;
            }
            catch (DbUnavailableException)
            {
                // Here you can display the error and prompt for user to re-enter database
                Console.WriteLine(" Database not found");
                return;
            }

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);


            while (true)
            {
                Console.WriteLine("Downloading data...");

                List<Device> vehicles = await api.CallAsync<List<Device>>("Get", typeof(Device));

                foreach (Device device in vehicles)
                {
                    string filename = device.SerialNumber + ".csv";
                    string path = Path.Combine(folder, filename);
                    if (!File.Exists(path))
                    {
                        using (FileStream fs = File.Create(path)) ;

                        //string csvHeader = string.Format(", {1}", );

                        File.WriteAllText(path, "ID,Timestamp, VIN,Coordinates,Odometer \n");
                    }

                    using (StreamWriter writer = new StreamWriter(path, true))
                    {
                        writer.WriteLine($"{device.SerialNumber}, {DateTime.UtcNow}");
                    };

                    Console.WriteLine("Serial number : " + device.SerialNumber);
                }

                Thread.Sleep(1000);
            }

        }
    }
}
