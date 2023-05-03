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
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

/* 
Developer: Kyle Watson
Date: 18/04/2023
Project Tasks/Goals
    /1. Program will fetch a list of all Steam apps which includes their name and ID from the Steam API using the GetSteamGamesAsync endpoint. 
    /2. Stores the information in a CSV file for backup purposes and to minimize api calls.
    /3. AppIDs are then pulled from the CSV file and stored in a List.
    4. The List is then looped through to pull the AppID which is then used in a call to the Steam API using the GetGameDetailsAsync & GetAppReviewsAsync endpoint.
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
            var details = await FetchAppDetailsAsync(appId);
            var reviews = await FetchAppReviewsAsync(appId);
            MergedData md = new(details, reviews);
            Console.ReadLine();


        }
        #region App List Methods
        // Returns the App List from the Steam API endpoint.
        public static async Task<List<SteamApp>> GetSteamAppsAsync()
        {
            using var httpClient = new HttpClient();
            string url = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";

            string response = await httpClient.GetStringAsync(url) ?? throw new Exception("Failed to retrieve data from the API.");
            JObject jsonResponse = JObject.Parse(response);

            JArray appsArray = (JArray)(jsonResponse["applist"]?["apps"] ?? throw new Exception("Failed to locate data within the JSON response."));
            List<SteamApp> appList = appsArray.ToObject<List<SteamApp>>() ?? throw new Exception("There was no object to store within the List.");
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
            List<SteamApp> steamApps = new();
            using (StreamReader sr = new("steam_games.csv"))
            {
                string? line;
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
                        SteamApp app = new(appId, name);
                        steamApps.Add(app);
                    }
                }
            }
            return steamApps;
        }
        #endregion
        #region API Retrieval Methods
        // Returns a deserialized response from the Steam app details API
        public static async Task<DetailsRoot> FetchAppDetailsAsync(int appId)
        {
            string steamAppDetailsUrl = $"https://store.steampowered.com/api/appdetails?appids={appId}";


            using HttpClient httpClient = new();

            HttpResponseMessage detailsResponse = await httpClient.GetAsync(steamAppDetailsUrl);
            string jsonDetailsResponse = await detailsResponse.Content.ReadAsStringAsync();
            string jsonDetailsResponseInner = ExtractInnerJson(jsonDetailsResponse);

            // Deserialize the JSON response

            DetailsRoot myDeserializedDetailsClass = JsonConvert.DeserializeObject<DetailsRoot>(jsonDetailsResponseInner) ?? throw new Exception("There was no data to deserialize.");

            return myDeserializedDetailsClass;
        }
        // Returns a deserialized response from the Steam app reviews API
        public static async Task<ReviewRoot> FetchAppReviewsAsync(int appId)
        {
            string steamAppReviewUrl = $"https://store.steampowered.com/appreviews/{appId}?json=1";


            using HttpClient httpClient = new();

            HttpResponseMessage reviewResponse = await httpClient.GetAsync(steamAppReviewUrl);
            string jsonReviewResponse = await reviewResponse.Content.ReadAsStringAsync();

            // Deserialize the JSON response

            ReviewRoot myDeserializedReviewClass = JsonConvert.DeserializeObject<ReviewRoot>(jsonReviewResponse) ?? throw new Exception("There was no data to deserialize.");

            return myDeserializedReviewClass;
        }
        public static string ExtractInnerJson(string jsonString)
        {
            JObject outerObject = JObject.Parse(jsonString);
            JProperty? firstProperty = outerObject.Properties().FirstOrDefault();

            // If the inner object is null, return
            if (firstProperty == null)
            {
                return string.Empty;
            }
            JObject innerObject = (JObject)firstProperty.Value;
            return innerObject.ToString();

        }
        #endregion

        public static string GetConnectionString(string connectionName)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            return configuration.GetConnectionString(connectionName) ?? throw new Exception("Unable to resolve connection string");
        }
    }

    // Creates a DbContext class that represents the database
    public class SteamDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Set the connection string for your database
            string connectionString = Program.GetConnectionString("SteamAppDatabase");
            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }

    #region GetSteamApps Class
    // SteamApps Class which stores app ID and name from the GetSteamApps Steam endpoint
    public class SteamApp
    {
        public int AppId { get; set; }
        public string Name { get; set; }

        public SteamApp(int appId, string name)
        {
            this.AppId = appId;
            this.Name = name;
        }
        public override string ToString()
        {
            return $"SteamApp(AppId={AppId}, GameName='{Name}')";
        }
    }
    #endregion
    #region MergedData Class
    // Merge Class - combines both deserialized API endpoints into a single object to allow a single return
    [NotMapped]
    public class MergedData
    {
        public DetailsRoot? DetailsObject { get; set; }

        public ReviewRoot? ReviewObject { get; set; }

        public MergedData() { }
        public MergedData(DetailsRoot dR, ReviewRoot rR)
        {

            DetailsObject = dR;
            ReviewObject = rR;
        }
    }
    #endregion
    #region Review Classes
    // Classes for deserializing results from the 
    public class ReviewRoot
    {
        [JsonProperty("success")]
        public int Success { get; set; }

        [JsonProperty("query_summary")]
        public QuerySummary? QuerySummary { get; set; }
    }
    public class QuerySummary
    {
        [JsonProperty("review_score_desc")]
        public string ReviewScoreDesc { get; set; } = "No user reivew";

        [JsonProperty("total_positive")]
        public int TotalPositive { get; set; }

        [JsonProperty("total_negative")]
        public int TotalNegative { get; set; }

        [JsonProperty("total_reviews")]
        public int TotalReviews { get; set; }
    }
    #endregion
    #region Details Classes
    // Details Classes
    public class DetailsRoot
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public Data? Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("steam_appid")]
        public int SteamAppid { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = "Not given";

        [JsonProperty("name")]
        public string Name { get; set; } = "Not given";

        [JsonProperty("required_age")]
        public int RequiredAge { get; set; }

        [JsonProperty("is_free")]
        public bool IsFree { get; set; } = false;

        [JsonProperty("detailed_description")]
        public string? DetailedDescription { get; set; }

        [JsonProperty("about_the_game")]
        public string? AboutTheGame { get; set; }

        [JsonProperty("short_description")]
        public string? ShortDescription { get; set; }

        [JsonProperty("supported_languages")]
        public string? SupportedLanguages { get; set; }

        [JsonProperty("header_image")]
        public string? HeaderImage { get; set; }

        [JsonProperty("website")]
        public string? Website { get; set; }

        [JsonProperty("pc_requirements")]
        public PcRequirements? PcRequirements { get; set; }

        [JsonProperty("mac_requirements")]
        public MacRequirements? MacRequirements { get; set; }

        [JsonProperty("linux_requirements")]
        public LinuxRequirements? LinuxRequirements { get; set; }

        [JsonProperty("developers")]
        public List<string>? Developers { get; set; }

        [JsonProperty("publishers")]
        public List<string>? Publishers { get; set; }

        [JsonProperty("price_overview")]
        public PriceOverview? PriceOverview { get; set; }

        [JsonProperty("platforms")]
        public Platforms? Platforms { get; set; }

        [JsonProperty("metacritic")]
        public Metacritic? Metacritic { get; set; }

        [JsonProperty("screenshots")]
        public List<Screenshot>? Screenshots { get; set; }

        [JsonProperty("movies")]
        public List<Movie>? Movies { get; set; }

        [JsonProperty("recommendations")]
        public Recommendations? Recommendations { get; set; }

        [JsonProperty("release_date")]
        public ReleaseDate? ReleaseDate { get; set; }

        [JsonProperty("support_info")]
        public SupportInfo? SupportInfo { get; set; }

        [JsonProperty("background")]
        public string? Background { get; set; }

        [JsonProperty("background_raw")]
        public string? BackgroundRaw { get; set; }
    }
    public class PcRequirements
    {
        [JsonProperty("minimum")]
        public string? Minimum { get; set; }
    }
    public class LinuxRequirements
    {
        [JsonProperty("minimum")]
        public string? Minimum { get; set; }
    }
    //public class Developer
    //{
    //    public string? name { get; set;}
    //}
    //public class Publisher
    //{
    //    public string? name { get; set; }
    //}
    public class MacRequirements
    {
        [JsonProperty("minimum")]
        public string? Minimum { get; set; }
    }

    public class Metacritic
    {
        [JsonProperty("score")]
        public int? Score { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }
    }

    public class Movie
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonProperty("webm")]
        public Webm? Webm { get; set; }

        [JsonProperty("mp4")]
        public Mp4? Mp4 { get; set; }

        [JsonProperty("highlight")]
        public bool Highlight { get; set; }
    }

    public class Mp4
    {
        [JsonProperty("480")]
        public string? _480 { get; set; }

        [JsonProperty("max")]
        public string? Max { get; set; }
    }

    public class Platforms
    {
        [JsonProperty("windows")]
        public bool Windows { get; set; } = false;

        [JsonProperty("mac")]
        public bool Mac { get; set; } = false;

        [JsonProperty("linux")]
        public bool Linux { get; set; } = false;
    }

    public class PriceOverview
    {
        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("initial")]
        public int Initial { get; set; }

        [JsonProperty("final")]
        public int Final { get; set; }

        [JsonProperty("discount_percent")]
        public int DiscountPercent { get; set; }

        [JsonProperty("initial_formatted")]
        public string? InitialFormatted { get; set; }

        [JsonProperty("final_formatted")]
        public string? FinalFormatted { get; set; }
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
        public string? Date { get; set; }
    }

    public class Screenshot
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("path_thumbnail")]
        public string? PathThumbnail { get; set; }

        [JsonProperty("path_full")]
        public string? PathFull { get; set; }
    }

    public class SupportInfo
    {
        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }
    }

    public class Webm
    {
        [JsonProperty("480")]
        public string? _480 { get; set; }

        [JsonProperty("max")]
        public string? Max { get; set; }
    }
    #endregion
}

