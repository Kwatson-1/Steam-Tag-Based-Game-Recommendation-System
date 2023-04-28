using CsvHelper;
using Newtonsoft.Json;
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
using Newtonsoft.Json.Converters;


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
        public static string steamWebApiKey = "81410A991EDD3F3DDDF9177C3DB453C9";
        public static async Task Main(string[] args)
        {
            //List<SteamApps> games = await GetSteamAppsAsync();
            //string outputPath = "steam_games.csv";
            //SaveSteamAppsToCsv(games, outputPath);
            //Console.WriteLine($"Saved {games.Count} games to {outputPath}");

            List<SteamApp> appList = GetSteamAppsFromCsv();
        }
        #region App List Methods
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
        #endregion
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

    #region Deserialisation Classes
    // Steam App Details Class for deserialisations
    public class JsonResponse
    {
        // Table Game
        [JsonProperty("type")]
        private string Type;
        [JsonProperty("name")]
        private string Name;
        [JsonProperty("steam_appid")]
        private int Steamappid;
        [JsonProperty("is_free")]
        private bool IsFree;
        [JsonProperty("detailed_description")]
        private string DetailedDescription;
        [JsonProperty("about_the_game")]
        private string AboutTheGame;
        [JsonProperty("short_description")]
        private string ShortDescription;
        [JsonProperty("supported_languages")]
        private string SupportedLanguages;
        [JsonProperty("header_image")]
        private string HeaderImage;
        [JsonProperty("website)")]
        private string Webiste;
        [JsonProperty("release_date")]
        private ReleaseDate ReleaseDate;

        // Table PC_Requirements
        [JsonProperty("pc_requirements")]
        private Requirements PcRequirements;
        [JsonProperty("mac_requirements")]
        private Requirements MacRequirements;
        [JsonProperty("linux_requirements")]
        private Requirements LinuxRequirements;

        // Table Developer
        [JsonProperty("developers")]
        private List<string> Developers;

        // Table Publisher
        [JsonProperty("publishers")]
        private List<string> Publishers;

        // Table Price_Overview
        [JsonProperty("price_overview")]
        private Price PriceOverview;

        // Table Platform
        [JsonProperty("platforms")]
        private Platform Platforms;

        // Table Metacritic
        [JsonProperty("metacritic")]
        private Metacritic Metacritic;

        // Table Screenshot
        [JsonProperty("screenshots")]
        private List<Screenshot> Screenshots;

        // Table Movie
        [JsonProperty("movies")]
        private Movie Movie;

        // Recommendations
        [JsonProperty("recommendations")]
        private ReviewScore ReviewScores;

        // Table Support_Info
        [JsonProperty("support_info")]
        private SupportInfo SupportInfo;

        // Table Background
        [JsonProperty("background")]
        private string background;
        [JsonProperty("background_raw")]
        private string backgroundRaw;
    }

    public class Requirements
    {
        [JsonProperty("minimum")]
        private string minimum;
    }

    public class Price
    {
        [JsonProperty("currency")]
        private string currency;
        [JsonProperty("discount_percent")]
        private int discountPercent;
        [JsonProperty("final_formatted")]
        private string final_formatted;
    }

    public class Platform
    {
        [JsonProperty("windows")]
        private Boolean windows;
        [JsonProperty("mac")]
        private Boolean mac;
        [JsonProperty("linux")]
        private Boolean linux;
    }
    public class Metacritic
    {
        [JsonProperty("score")]
        private int score;
        [JsonProperty("url")]
        private string url;
    }
    public class Screenshot
    {
        [JsonProperty("id")]
        private int id;
        [JsonProperty("path_thumbnail")]
        private string pathThumbnail;
        [JsonProperty("path_full")]
        private string pathFull;
    }
    public class Movie
    {
        [JsonProperty("id")]
        private int movieId;
        [JsonProperty("name")]
        private string name;
        [JsonProperty("thumbnail")]
        private string thumbnail;
        [JsonProperty("webm")]
        private WebmData webm;
        [JsonProperty("mp4")]
        private Mp4Data mp4;
        [JsonProperty("highlight")]
        private Boolean highlight;
    }
    public class WebmData
    {
        [JsonProperty("480")]
        private string _480;
        [JsonProperty("max")]
        private string max;
    }
    public class Mp4Data
    {
        [JsonProperty("480")]
        private string _480;
        [JsonProperty("max")]
        private string max;
    }
    public class ReviewScore
    {
        [JsonProperty("review_score_desc")]
        private string reviewScoreDesc;
        [JsonProperty("total_reviews")]
        private int totalReviews;
        [JsonProperty("total_positive")]
        private int totalPositive;
        [JsonProperty("total_negative")]
        private int totalNegative;
    }
    public class ReleaseDate
    {
        [JsonProperty("date")]
        private string date;
    }

    public class SupportInfo
    {
        [JsonProperty("url")]
        private string url;
        [JsonProperty("email")]
        private string email;
    }
    #endregion
}

    