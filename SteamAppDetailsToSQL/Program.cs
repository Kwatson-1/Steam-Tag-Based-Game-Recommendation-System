using CsvHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


/* 
Developer: Kyle Watson
Date: 18/04/2023
Project Tasks/Goals
    1. Program will fetch a list of all Steam apps which includes their name and ID from the Steam API using the GetSteamGamesAsync endpoint. 
    2. Stores the information in a CSV file for backup purposes and to minimize api calls.
    3. AppIDs are then pulled from the CSV file and stored in a List.
    4. The List is then looped through to pull the AppID which is then used in a call to the Steam API using the GetGameDetailsAsync endpoint.
    4a. Use Task.Delay to introduce a delay between each call to avoid potential issues or bans.
    5. Game information is stored in a JSON file for backup purposes and to minimize api calls.
    6. Call back the data fro the JSON file and parse the JSON response for each app and extract the necessary data.
    7. Use a library to connect the SQL Server dataase and insert the extracted data.
*/
namespace SteamAppDetailsToSQL
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            List<SteamApps> games = await GetSteamAppsAsync();
            string outputPath = "steam_games.csv";
            SaveSteamAppsToCsv(games, outputPath);

            Console.WriteLine($"Saved {games.Count} games to {outputPath}");
        }
        // Returns the App List from the Steam API endpoint.
        public static async Task<List<SteamApps>> GetSteamAppsAsync()
        {
            using var httpClient = new HttpClient();
            string url = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";
            string response = await httpClient.GetStringAsync(url);

            JObject jsonResponse = JObject.Parse(response);
            JArray appsArray = (JArray)jsonResponse["applist"]["apps"];

            List<SteamApps> appList = appsArray.ToObject<List<SteamApps>>();
            return appList;
        }
        // Saves
        public static void SaveSteamAppsToCsv(List<SteamApps> apps, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(apps);
        }
    }
    public class SteamApps
    {
        public int appId { get; set; }
        public string name { get; set; }
    }
}

