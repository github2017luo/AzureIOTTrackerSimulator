using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Windows;

namespace AzureIOTTrackerSimulator
{
    public class Device
    {
        private string deviceId;
        private DeviceClient client;
        private int locationCounter = 0;
        private List<Location> locations;

        public bool Complete { get; set; } = false;
        public int MessageCount { get; set; }

        public Device(string deviceId)
        {
            this.deviceId = deviceId;
        }

        public async Task Initialize()
        {
            string azureScopeId = ConfigurationManager.AppSettings["ScopeId"];
            string azureDeviceId = ConfigurationManager.AppSettings[deviceId + "Id"];
            string azureDevicekey = ConfigurationManager.AppSettings[deviceId + "Key"];
            Connection connection = new Connection();
            await connection.Connect(azureScopeId, azureDeviceId, azureDevicekey);
            client = connection.deviceClient;
            Logger.Log(deviceId + ": " + azureDeviceId + ";" + azureDevicekey);
        }

        public async Task InitializeLocations(string origin, string destination)
        {
            locations = await GetCordinates(origin, destination);
            locationCounter = 0;
            MessageCount = locations.Count;
        }

        public async void SendMessage()
        {
            if (locationCounter < locations.Count)
            {
                var telemetryDataPoint = new TelemetryDataPoint()
                {
                    location = locations[locationCounter]
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                Logger.Log(deviceId + ": " + messageString);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await client.SendEventAsync(message);
                locationCounter++;
                if (locationCounter == locations.Count)
                {
                    Complete = true;
                    Logger.Log(deviceId + ": End");
                }
            }
        }

        private async Task<List<Location>> GetCordinates(string origin, string destination)
        {
            if(string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
            {
                Complete = true;
                return new List<Location>();
            }

            string response = await GetMapsResponse(origin, destination);

            List<Location> locations = new List<Location>();
            dynamic directions = JsonConvert.DeserializeObject(response);
            if(directions.routes.Count > 0)
            {
                dynamic leg = directions.routes[0].legs[0];
                locations.Add(new Location()
                {
                    lat = leg.start_location.lat,
                    lon = leg.start_location.lng
                });

                dynamic steps = leg.steps;
                for(int counter = 0; counter < steps.Count; counter++)
                {
                    if (steps[counter].distance.value > 20000)
                    {
                        locations.Add(new Location()
                        {
                            lat = steps[counter].end_location.lat,
                            lon = steps[counter].end_location.lng
                        });
                    }
                }

                locations.Add(new Location()
                {
                    lat = leg.end_location.lat,
                    lon = leg.end_location.lng
                });
            }

            return locations;
        }

        private async Task<string> GetMapsResponse(string origin, string destination)
        {
            string requestUri = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0}&destination={1}&key={2}",
                WebUtility.UrlEncode(origin), WebUtility.UrlEncode(destination), ConfigurationManager.AppSettings["googleApi"]);

            HttpClient httpClient = new HttpClient();
            var response = await httpClient.PostAsync(requestUri, null);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
