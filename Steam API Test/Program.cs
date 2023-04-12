using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SteamGameInfoRetriever
{
    class Program
    {
        private static readonly string appId = "435150"; // Divinity 2: Original Sin AppID
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            try
            {
                string gameInfoJson = await GetGameInfoAsync(appId);
                Console.WriteLine("Game information for Divinity 2: Original Sin:");
                Console.WriteLine(gameInfoJson);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static async Task<string> GetGameInfoAsync(string appId)
        {
            string requestUrl = $"https://store.steampowered.com/api/appdetails/?appids={appId}";
            HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject jsonResponse = JObject.Parse(content);
                JObject gameInfo = (JObject)jsonResponse[appId]["data"];
                return gameInfo.ToString();
            }
            else
            {
                throw new Exception("Failed to retrieve game information.");
            }
        }
    }
}