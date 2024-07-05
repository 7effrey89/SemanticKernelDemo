using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Newtonsoft.Json.Linq;

namespace SemanticKernel.Plugins
{
    internal class WeatherServicePlugin
    {
        private static readonly HttpClient httpClient = new HttpClient();
        static WeatherServicePlugin()
        {
            // Set a default User-Agent header
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
        }

        [KernelFunction("Weather")]
        [Description("Provide the weather forecast e.g. temperature and windspeed in km/h for a place using latitude and longitude.")]
        public async Task<string> GetWeatherAsync([Description("latitude of a location to get the weather forecast for")] double latitude, [Description("longitude of a location to get the weather forecast for")] double longitude)
        {
            Console.WriteLine($"-----------------------------------------------");
            Console.WriteLine("Initiating Function: GetWeatherAsync");
            try
            {
                //format the coordinates to always show . despite local
                string lat = latitude.ToString("0.00", CultureInfo.InvariantCulture);
                string lon = longitude.ToString("0.00", CultureInfo.InvariantCulture);

                // Construct the API endpoint
                string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";

                // Make the GET request
                HttpResponseMessage response = await httpClient.GetAsync(url);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Parse the JSON response
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(responseBody);
                    JObject currentWeather = (JObject)data["current_weather"];

                    //if (currentWeather != null)
                    //{
                    //    double temperature = currentWeather.Value<double>("temperature");
                    //    double windSpeed = currentWeather.Value<double>("windspeed");
                    //    string time = currentWeather.Value<string>("time") ?? DateTime.UtcNow.ToString("o");

                    //    //Console.WriteLine($"Current weather at {latitude}, {longitude}:");
                    //    //Console.WriteLine($"Temperature: {temperature}°C");
                    //    //Console.WriteLine($"Wind Speed: {windSpeed} km/h");
                    //    //Console.WriteLine($"Time: {time}");
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Current weather data not found.");
                    //}

                    Console.WriteLine($"REST API returned: {responseBody}\n-----------------------------------------------");

                    return data.ToString();
                }
                else
                {
                    Console.WriteLine("Failed to retrieve data.");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                // Handle any errors that may have occurred
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }
        public static async Task test_Weather_REST_API()
        {
            WeatherServicePlugin service = new WeatherServicePlugin();
            string result = await service.GetWeatherAsync(latitude: 55.67594000, longitude: 12.56553000);

            if (result.Length>0)
            {
                Console.WriteLine($"Coordinates for Copenhagen: { result}");
            }
            else
            {
                Console.WriteLine("Failed to retrieve.");
            }
        }
    }

}
