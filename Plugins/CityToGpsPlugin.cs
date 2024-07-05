using Microsoft.SemanticKernel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernel.Plugins
{
    internal class CityToGpsPlugin
    {
        private static readonly HttpClient httpClient = new HttpClient();
        static CityToGpsPlugin()
        {
            // Set a default User-Agent header
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
        }

        [KernelFunction("Get_Latitude_Longitude_For_City")]
        [Description("Provide the latitude and longitude coordinates for a city")]
        public async Task<string> GetCoordinatesAsync([Description("The name of the city to look up the coordinates for")] string city)
        {
            Console.WriteLine($"-----------------------------------------------");
            Console.WriteLine("Initiating Function: GetCoordinatesAsync");
            try
            {
                // Construct the API endpoint
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(city)}&format=json&limit=1";

                // Make the GET request
                HttpResponseMessage response = await httpClient.GetAsync(url);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Parse the JSON response
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JArray data = JArray.Parse(responseBody);

                    Console.WriteLine($"REST API returned: {responseBody}\n-----------------------------------------------");

                    return responseBody;

                }
                else
                {
                    string failed = "Failed to retrieve data.";
                    Console.WriteLine(failed);
                    return failed;
                }
            }
            catch (HttpRequestException e)
            {
                // Handle any errors that may have occurred
                Console.WriteLine($"Request error: {e.Message}");
                return e.Message;
            }
        }
        private async Task<(double Latitude, double Longitude)> GetCoordinatesAsDoubles(string responseBody)
        {
            try
            {
                JArray data = JArray.Parse(responseBody);

                if (data.Count > 0)
                {
                    double latitude = data[0].Value<double>("lat");
                    double longitude = data[0].Value<double>("lon");

                    return (latitude, longitude);
                }
                else
                {
                    Console.WriteLine($"Coordinates not found.");
                    return (0, 0);
                }

            }
            catch (HttpRequestException e)
            {
                // Handle any errors that may have occurred
                Console.WriteLine($"Request error: {e.Message}");
                return (0, 0);
            }
        }

        public static async Task test_LatLong_REST_API()
        {
            CityToGpsPlugin geocodingService = new CityToGpsPlugin();
            string result = await geocodingService.GetCoordinatesAsync("Copenhagen");

            Console.WriteLine(result);
        }
    }
}
