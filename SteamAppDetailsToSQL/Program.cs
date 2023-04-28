﻿using CsvHelper;
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
using System.Text.Json.Serialization;


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

            //List<SteamApp> appList = GetSteamAppsFromCsv();
            int appId = 294100; // Replace with the desired app ID
            var data = await FetchAppDetailsAsync(appId);
            Console.WriteLine(data.ToString());
            Console.ReadLine();

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

        public static async Task<MergeClass> FetchAppDetailsAsync(int appId)
        {
            string steamAppDetailsUrl = $"https://store.steampowered.com/api/appdetails?appids={appId}";
            string steamAppReviewUrl = $"https://store.steampowered.com/appreviews/{appId}?json=1";


            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage detailsResponse = await httpClient.GetAsync(steamAppDetailsUrl);
            string jsonDetailsResponse = await detailsResponse.Content.ReadAsStringAsync();
            string jsonDetailsResponseInner = ExtractInnerJson(jsonDetailsResponse);
            HttpResponseMessage reviewResponse = await httpClient.GetAsync(steamAppReviewUrl);
            string jsonReviewResponse = await reviewResponse.Content.ReadAsStringAsync();

            // Deserialize the JSON response

            DetailsRoot myDeserializedDetailsClass = JsonConvert.DeserializeObject<DetailsRoot>(jsonDetailsResponseInner);
            ReviewRoot myDeserializedReviewClass = JsonConvert.DeserializeObject<ReviewRoot>(jsonReviewResponse);

            MergeClass mergedAppData = new MergeClass(myDeserializedDetailsClass, myDeserializedReviewClass);

            return mergedAppData;
        }

        public static string ExtractInnerJson(string jsonString)
        {
            JObject outerObject = JObject.Parse(jsonString);
            JProperty firstProperty = outerObject.Properties().FirstOrDefault();
            if (firstProperty != null)
            {
                JObject innerObject = (JObject)firstProperty.Value;
                return innerObject.ToString();
            }

            return null;
        }
    }
    #region GetSteamApps Class
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
    #endregion

    // Merge Class

    public class MergeClass
    {

        public DetailsRoot DetailsObject { get; set; }

        public ReviewRoot ReviewObject { get; set; }
        public MergeClass(DetailsRoot dR, ReviewRoot rR)
        {

            DetailsObject = dR;
            ReviewObject = rR;
        }
    }


    // Review Classes
        public class ReviewRoot
        {
            [JsonProperty("success")]
            public int Success { get; set; }

            [JsonProperty("query_summary")]
            public QuerySummary QuerySummary { get; set; }
        }
        public class QuerySummary
        {
            [JsonProperty("num_reviews")]
            public int NumReviews { get; set; }

            [JsonProperty("review_score")]
            public int ReviewScore { get; set; }

            [JsonProperty("review_score_desc")]
            public string ReviewScoreDesc { get; set; }

            [JsonProperty("total_positive")]
            public int TotalPositive { get; set; }

            [JsonProperty("total_negative")]
            public int TotalNegative { get; set; }

            [JsonProperty("total_reviews")]
            public int TotalReviews { get; set; }
        }



    // Details Classes
    public class DetailsRoot
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("steam_appid")]
        public int SteamAppid { get; set; }

        [JsonProperty("required_age")]
        public int RequiredAge { get; set; }

        [JsonProperty("is_free")]
        public bool IsFree { get; set; }


        [JsonProperty("detailed_description")]
        public string DetailedDescription { get; set; }

        [JsonProperty("about_the_game")]
        public string AboutTheGame { get; set; }

        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }

        [JsonProperty("supported_languages")]
        public string SupportedLanguages { get; set; }

        [JsonProperty("header_image")]
        public string HeaderImage { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("pc_requirements")]
        public PcRequirements PcRequirements { get; set; }

        [JsonProperty("mac_requirements")]
        public MacRequirements MacRequirements { get; set; }

        [JsonProperty("linux_requirements")]
        public LinuxRequirements LinuxRequirements { get; set; }

        [JsonProperty("developers")]
        public List<string> Developers { get; set; }

        [JsonProperty("publishers")]
        public List<string> Publishers { get; set; }

        [JsonProperty("price_overview")]
        public PriceOverview PriceOverview { get; set; }


        [JsonProperty("platforms")]
        public Platforms Platforms { get; set; }

        [JsonProperty("metacritic")]
        public Metacritic Metacritic { get; set; }

        [JsonProperty("screenshots")]
        public List<Screenshot> Screenshots { get; set; }

        [JsonProperty("movies")]
        public List<Movie> Movies { get; set; }

        [JsonProperty("recommendations")]
        public Recommendations Recommendations { get; set; }

        [JsonProperty("release_date")]
        public ReleaseDate ReleaseDate { get; set; }

        [JsonProperty("support_info")]
        public SupportInfo SupportInfo { get; set; }

        [JsonProperty("background")]
        public string Background { get; set; }

        [JsonProperty("background_raw")]
        public string BackgroundRaw { get; set; }

    }


    public class LinuxRequirements
    {
        [JsonProperty("minimum")]
        public string Minimum { get; set; }
    }

    public class MacRequirements
    {
        [JsonProperty("minimum")]
        public string Minimum { get; set; }
    }

    public class Metacritic
    {
        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Movie
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("webm")]
        public Webm Webm { get; set; }

        [JsonProperty("mp4")]
        public Mp4 Mp4 { get; set; }

        [JsonProperty("highlight")]
        public bool Highlight { get; set; }
    }

    public class Mp4
    {
        [JsonProperty("480")]
        public string _480 { get; set; }

        [JsonProperty("max")]
        public string Max { get; set; }
    }



    public class PcRequirements
    {
        [JsonProperty("minimum")]
        public string Minimum { get; set; }
    }

    public class Platforms
    {
        [JsonProperty("windows")]
        public bool Windows { get; set; }

        [JsonProperty("mac")]
        public bool Mac { get; set; }

        [JsonProperty("linux")]
        public bool Linux { get; set; }
    }

    public class PriceOverview
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("initial")]
        public int Initial { get; set; }

        [JsonProperty("final")]
        public int Final { get; set; }

        [JsonProperty("discount_percent")]
        public int DiscountPercent { get; set; }

        [JsonProperty("initial_formatted")]
        public string InitialFormatted { get; set; }

        [JsonProperty("final_formatted")]
        public string FinalFormatted { get; set; }
    }

    public class Recommendations
    {
        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class ReleaseDate
    {
        [JsonProperty("coming_soon")]
        public bool ComingSoon { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }
    }


    public class Screenshot
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("path_thumbnail")]
        public string PathThumbnail { get; set; }

        [JsonProperty("path_full")]
        public string PathFull { get; set; }
    }



    public class SupportInfo
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class Webm
    {
        [JsonProperty("480")]
        public string _480 { get; set; }

        [JsonProperty("max")]
        public string Max { get; set; }
    }



}

    