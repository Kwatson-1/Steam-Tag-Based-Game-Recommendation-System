﻿using CsvHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


/* 
Developer: Kyle Watson
Date: 18/04/2023
Project Tasks/Goals
    /1. Program will fetch a list of all Steam apps which includes their name and ID from the Steam API using the GetSteamGamesAsync endpoint. 
    /2. Stores the information in a CSV file for backup purposes and to minimize api calls.
    /3. AppIDs are then pulled from the CSV file and stored in a List.
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
            //List<SteamApps> games = await GetSteamAppsAsync();
            //string outputPath = "steam_games.csv";
            //SaveSteamAppsToCsv(games, outputPath);
            //Console.WriteLine($"Saved {games.Count} games to {outputPath}");

            List<SteamApp> appList = GetSteamAppsFromCsv();
        }
        // Returns the App List from the Steam API endpoint.
        public static async Task<List<SteamApp>> GetSteamAppsAsync()
        {
            using var httpClient = new HttpClient();
            string url = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";
            string response = await httpClient.GetStringAsync(url);

            JObject jsonResponse = JObject.Parse(response);
            JArray appsArray = (JArray)jsonResponse["applist"]["apps"];

            List<SteamApp> appList = appsArray.ToObject<List<SteamApp>>();
            return appList;
        }
        // Writes the Steam apps list to a CSV file.
        public static void SaveSteamAppsToCsv(List<SteamApp> apps, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(apps);
        }
        // Reads the Steam apps from the CSV file into a List of Class 'SteamApps'.
        // CSV file may require manual removal of records that are not in the correct format due to testing by the Steam development team.
        public static List<SteamApp> GetSteamAppsFromCsv()
        {
            List<SteamApp> steamApps = new List<SteamApp>();
            using (StreamReader sr = new StreamReader("steam_games.csv"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] values = line.Split(',');

                    if (values.Length < 2)
                    {
                        continue; // Skip the line if it does not have at least two values.
                    }

                    if (int.TryParse(values[0], out int appId) && !string.IsNullOrEmpty(values[1]))
                    {
                        string name = values[1].Trim(); // Trim any whitespace around the name.
                        SteamApp app = new SteamApp(appId, name);
                        steamApps.Add(app);
                    }
                }
            }
            return steamApps;
        }
        // Call of the GetAppDetails endpoint by iterating through each app ID in the app ID List of Class.
        public static Task<List<>>


    }
    

    // SteamApps Class which stores app ID and name from the GetSteamApps Steam endpoint
    public class SteamApp
    {
        public int appId { get; set; }
        public string name { get; set; }

        public SteamApp(int appId, string name)
        {
            this.appId = appId;
            this.name = name;
        }
        public override string ToString()
        {
            return $"SteamApp(AppId={appId}, GameName='{name}')";
        }
    }


    // Steam App Details
    public class Game
    {
        public int Id { get; set; }
        public int SteamAppId { get; set; }
        public string Name { get; set; }
        public bool IsFree { get; set; }
        public string DetailedDescription { get; set; }
        public string AboutTheGame { get; set; }
        public string ShortDescription { get; set; }
        public string HeaderImage { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int TotalRecommendations { get; set; }
    }

    public class Developer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Publisher
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GameDeveloper
    {
        public int GameId { get; set; }
        public int DeveloperId { get; set; }
    }

    public class GamePublisher
    {
        public int GameId { get; set; }
        public int PublisherId { get; set; }
    }

    public class Platform
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GamePlatform
    {
        public int GameId { get; set; }
        public int PlatformId { get; set; }
    }

    public class PriceOverview
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string Currency { get; set; }
        public int DiscountPercent { get; set; }
        public string FinalFormatted { get; set; }
    }

    public class Screenshot
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string PathThumbnail { get; set; }
        public string PathFull { get; set; }
    }

    public class Movie
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public string Webm480 { get; set; }
        public string WebmMax { get; set; }
        public string Mp4480 { get; set; }
        public string Mp4Max { get; set; }
    }

}

