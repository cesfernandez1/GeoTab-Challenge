using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

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

                List<Device> devices = await api.CallAsync<List<Device>>("Get", typeof(Device));

                foreach (GoDevice device in devices)
                {
                    string filename = device.SerialNumber + ".csv";
                    string path = Path.Combine(folder, filename);
                    if (!File.Exists(path))
                    {
                        using (FileStream fs = File.Create(path)) ;

                        //string csvHeader = string.Format(", {1}", );

                        File.WriteAllText(path, "ID,Timestamp, VIN,Coordinates,Odometer \n");
                    }

                    var results = await api.CallAsync<List<DeviceStatusInfo>>("Get", typeof(DeviceStatusInfo), new
                    {
                        search = new DeviceStatusInfoSearch
                        {
                            DeviceSearch = new DeviceSearch
                            {
                                Id = device.Id
                            }
                        }
                    });

                    if(results.Count <= 0)
                    {
                        continue;
                    }

                    var statusDataSearch = new StatusDataSearch
                    {
                        DeviceSearch = new DeviceSearch(device.Id),
                        DiagnosticSearch = new DiagnosticSearch(KnownId.DiagnosticOdometerAdjustmentId),
                        FromDate = DateTime.MaxValue
                    };

                    // Retrieve the odometer status data
                    IList<StatusData> statusData = await api.CallAsync<IList<StatusData>>("Get", typeof(StatusData), new { search = statusDataSearch });

                    var odometerREadings = statusData[0].Data ?? 0;

                    DeviceStatusInfo deviceStatus = results[0];

                    var isMetric = RegionInfo.CurrentRegion.IsMetric;


                    using (StreamWriter writer = new StreamWriter(path, true))
                    {
                        string coordinates = string.Format("(xy) {0}   {1}", deviceStatus.Latitude, deviceStatus.Longitude);

                        var odometer = Math.Round(isMetric ? odometerREadings : Distance.ToImperial(odometerREadings / 1000), 0);

                        writer.WriteLine($"{device.Id}, {DateTime.Now} , {device.SerialNumber} , {coordinates} , {odometer}");
                    };
                }

                Thread.Sleep(1000);
            }

        }
    }
}
