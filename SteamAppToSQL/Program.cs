using CsvHelper;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Formats.Asn1;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Globalization;
//using SteamAppDatabase.Entities;
//using SteamAppDatabase.Models;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.Collections;
using Microsoft.Extensions.Hosting;

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
        //public static string steamWebApiKey = "81410A991EDD3F3DDDF9177C3DB453C9";
        public static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            //var appList = UpdateGetAppList();
            //var appDataList = GetAllAppData(await appList);

            // Testing for individual app
            //int appId = 294100; // Replace with the desired app ID
            //var details = await FetchAppDetailsAsync(appId);
            //var reviews = await FetchAppReviewsAsync(appId);
            //MergedData md = new(details, reviews);
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure services
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        //.AddUserSecrets<Program>() // Add secrets from the Secret Manager
                        .Build();

                    AppSettings appSettings = new AppSettings(
                        configuration.GetValue<string>("SteamAppDatabase"),
                        configuration.GetValue<string>("SteamWebApi")
                    );

                    services.AddSingleton(appSettings);
                    services.AddDbContext<SteamDbContext>(options => options.UseSqlServer(appSettings.SteamAppDatabase));

                    // Other service registrations here:

                });

        #region App List Methods
        // Gets the steam app list, updates the output file and returns the new List<SteamApp> object
        public static async Task<List<SteamApp>> UpdateGetAppList()
        {
            List<SteamApp> games = await GetSteamAppsAsync();
            string outputPath = "steam_games.csv";
            SaveSteamAppsToCsv(games, outputPath);
            Console.WriteLine($"Saved {games.Count} games to {outputPath}");
            List<SteamApp> appList = GetSteamAppsFromCsv();
            return appList;
        }

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
        // Writes Steam apps list to a CSV file.
        public static void SaveSteamAppsToCsv(List<SteamApp> apps, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(apps);
        }
        // Reads the Steam apps from the CSV file into a List of Class 'SteamApps'.
        // CSV file may require manual removal of records that are not in the correct format due to specific test cases used by the Steam development team or delete/removed apps.
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
        public static async Task<List<MergedData>> GetAllAppData(List<SteamApp> appList)
        {
            List<MergedData> appDataList = new();
            foreach (var app in appList)
            {
                var details = await FetchAppDetailsAsync(app.AppId);
                var reviews = await FetchAppReviewsAsync(app.AppId);
                MergedData md = new(details, reviews);
                appDataList.Add(md);
            }
            return appDataList;
        }
        public OrmGame MapMergedDataToOrmGame(MergedData mergedData)
        {
            OrmGame ormGame = new();
            var detailsData = mergedData.DetailsObject.Data;
            ormGame.SteamAppId = detailsData.SteamAppId;
            ormGame.Type = detailsData.Type;
            ormGame.Name = detailsData.Name;
            ormGame.RequiredAge = detailsData.RequiredAge;
            ormGame.IsFree = detailsData.IsFree;
            ormGame.DetailedDescription = detailsData.DetailedDescription;
            ormGame.AboutTheGame = detailsData.AboutTheGame;
            ormGame.ShortDescription = detailsData.ShortDescription;
            ormGame.HeaderImage = detailsData.HeaderImage;
            ormGame.ReleaseDate = detailsData.ReleaseDate.ToString();

            ormGame.Platforms = new OrmPlatform
            {
                Windows = detailsData.Platforms.Windows,
                Mac = detailsData.Platforms.Mac,
                Linux = detailsData.Platforms.Linux,
                OrmGame = ormGame
            };

            ormGame.SupportInfo = new OrmSupportInfo
            {
                Url = detailsData.SupportInfo.Url,
                Email = detailsData.SupportInfo.Email,
                OrmGame = ormGame
            };

            ormGame.PriceOverview = new OrmPriceOverview
            {
                Currency = detailsData.PriceOverview.Currency,
                DiscountPercent = detailsData.PriceOverview.DiscountPercent,
                FinalFormatted = detailsData.PriceOverview.FinalFormatted,
                OrmGame = ormGame
            };

            ormGame.Recommendations = new OrmRecommendations
            {
                ReviewScoreDesc = mergedData.ReviewObject.QuerySummary.ReviewScoreDesc,
                TotalReviews = mergedData.ReviewObject.QuerySummary.TotalReviews,
                TotalPositive = mergedData.ReviewObject.QuerySummary.TotalPositive,
                TotalNegative = mergedData.ReviewObject.QuerySummary.TotalNegative,
                OrmGame = ormGame
            };

            ormGame.Metacritic = new OrmMetacritic
            {
                Score = detailsData.Metacritic.Score,
                Url = detailsData.Metacritic.Url,
                OrmGame = ormGame
            };

            ormGame.Background = new OrmBackground
            {
                Background = detailsData.Background,
                BackgroundRaw = detailsData.BackgroundRaw,
                OrmGame = ormGame
            };

            // ... The rest of your mapping logic ...

            //if (mergedData.ReviewObject != null && mergedData.ReviewObject.QuerySummary != null)
            //{
            //    var reviewSummary = mergedData.ReviewObject.QuerySummary;

            //    ormGame.Recommendations = new OrmRecommendations
            //    {
            //        OrmGame = ormGame,
            //        ReviewScoreDesc = reviewSummary.ReviewScoreDesc,
            //        TotalReviews = reviewSummary.TotalReviews,
            //        TotalPositive = reviewSummary.TotalPositive,
            //        TotalNegative = reviewSummary.TotalNegative
            //    };
            //}

            //// Mapping for OrmRequirements
            //if (mergedData.RequirementsObject != null)
            //{
            //    var requirementsData = mergedData.RequirementsObject;

            //    ormGame.Requirements = new OrmRequirements
            //    {
            //        OrmGame = ormGame,
            //        Minimum = requirementsData.Minimum,
            //        Recommended = requirementsData.Recommended
            //        // Map other properties...
            //    };
            //}

            //// Mapping for OrmPriceOverview
            //if (mergedData.PriceOverviewObject != null)
            //{
            //    var priceOverviewData = mergedData.PriceOverviewObject;

            //    ormGame.PriceOverview = new OrmPriceOverview
            //    {
            //        OrmGame = ormGame,
            //        Currency = priceOverviewData.Currency,
            //        Initial = priceOverviewData.Initial,
            //        Final = priceOverviewData.Final,
            //        DiscountPercent = priceOverviewData.DiscountPercent
            //        // Map other properties...
            //    };
            //}

            return ormGame;
        }
    }
    #region App settings
    public class AppSettings
    {
        public string SteamAppDatabase { get; set; }
        public string SteamWebApi { get; set; }
        // Overloaded constructor
        public AppSettings(string steamAppDatabase, string steamWebApi)
        {
            SteamAppDatabase = steamAppDatabase;
            SteamWebApi = steamWebApi;
        }
    }
    #endregion
    #region SteamDbContext
    // Creates a DbContext class that represents the database
    public class SteamDbContext : DbContext
    {
        private readonly AppSettings _appSettings;

        public SteamDbContext(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        public DbSet<OrmGame> Game { get; set; }
        public DbSet<OrmDeveloper> Developers { get; set; }
        public DbSet<OrmPublisher> Publishers { get; set; }
        public DbSet<OrmGameDeveloper> GameDevelopers { get; set; }
        public DbSet<OrmGamePublisher> GamePublishers { get; set; }
        public DbSet<OrmRequirements> Requirements { get; set; }
        public DbSet<OrmPlatform> Platforms { get; set; }
        public DbSet<OrmPriceOverview> PriceOverview { get; set; }
        public DbSet<OrmMetacritic> Metacritic { get; set; }
        public DbSet<OrmScreenshot> Screenshots { get; set; }
        public DbSet<OrmMovie> Movies { get; set; }
        public DbSet<OrmRecommendations> Recommendations { get; set; }
        public DbSet<OrmSupportInfo> SupportInfo { get; set; }
        public DbSet<OrmBackground> Backgrounds { get; set; }
        public DbSet<OrmLanguage> Language { get; set; }
        public DbSet<OrmGameLanguage> GameLanguages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Set the connection string for your database
            //string connectionString = Program.GetConnectionString("SteamAppDatabase");
            string connectionString = _appSettings.SteamAppDatabase;
            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrmGame>()
                .HasIndex(g => g.SteamAppId)
                .IsUnique();
            modelBuilder.Entity<OrmGameDeveloper>()
                .HasKey(gd => new { gd.GameAppId, gd.DeveloperId });
            modelBuilder.Entity<OrmGamePublisher>()
                .HasKey(gp => new { gp.GameAppId, gp.PublisherId });
            modelBuilder.Entity<OrmGameLanguage>()
                .HasKey(gl => new { gl.GameAppId, gl.LanguageId });
            modelBuilder.Entity<OrmRequirements>()
                .HasOne(r => r.OrmGame)
                .WithMany(g => g.Requirements)
                .HasForeignKey(r => r.GameAppId);
            modelBuilder.Entity<OrmPlatform>()
                .HasOne(m => m.OrmGame)
                .WithOne(g => g.Platforms)
                .HasForeignKey<OrmPlatform>(m => m.GameAppId);
            modelBuilder.Entity<OrmPriceOverview>()
                .HasOne(p => p.OrmGame)
                .WithOne(g => g.PriceOverview)
                .HasForeignKey<OrmPriceOverview>(p => p.GameAppId);
            modelBuilder.Entity<OrmMetacritic>()
                .HasOne(m => m.OrmGame)
                .WithOne(g => g.Metacritic)
                .HasForeignKey<OrmMetacritic>(m => m.GameAppId);
            modelBuilder.Entity<OrmScreenshot>()
                .HasOne(s => s.OrmGame)
                .WithMany(g => g.Screenshots)
                .HasForeignKey(s => s.GameAppId);
            modelBuilder.Entity<OrmMovie>()
                .HasOne(m => m.OrmGame)
                .WithMany(g => g.Movies)
                .HasForeignKey(m => m.GameAppId);
            modelBuilder.Entity<OrmRecommendations>()
                .HasOne(r => r.OrmGame)
                .WithOne(g => g.Recommendations)
                .HasForeignKey<OrmRecommendations>(r => r.GameAppId);
            modelBuilder.Entity<OrmSupportInfo>()
                .HasOne(s => s.OrmGame)
                .WithOne(g => g.SupportInfo)
                .HasForeignKey<OrmSupportInfo>(s => s.GameAppId);
            modelBuilder.Entity<OrmBackground>()
                .HasOne(b => b.OrmGame)
                .WithOne(g => g.Background)
                .HasForeignKey<OrmBackground>(b => b.GameAppId);
            modelBuilder.Entity<OrmGame>()
                .HasMany(g => g.GameDevelopers)
                .WithOne(gd => gd.OrmGame)
                .HasForeignKey(gd => gd.GameAppId);
            modelBuilder.Entity<OrmGame>()
                .HasMany(g => g.GamePublishers)
                .WithOne(gp => gp.OrmGame)
                .HasForeignKey(gp => gp.GameAppId);
            modelBuilder.Entity<OrmDeveloper>()
                .HasMany(d => d.GameDevelopers)
                .WithOne(gd => gd.OrmDeveloper)
                .HasForeignKey(gd => gd.DeveloperId);
            modelBuilder.Entity<OrmPublisher>()
                .HasMany(p => p.GamePublishers)
                .WithOne(gp => gp.OrmPublisher)
                .HasForeignKey(gp => gp.PublisherId);
            modelBuilder.Entity<OrmLanguage>()
                .HasMany(l => l.GameLanguages)
                .WithOne(g => g.OrmLanguage)
                .HasForeignKey(l => l.LanguageId);
        }
    }
    #endregion
    #region ORM classes
    [Table("Game")]
    public class OrmGame
    {
        [Key]
        public int SteamAppId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int RequiredAge { get; set; }
        public bool IsFree { get; set; }
        public string DetailedDescription { get; set; }
        public string AboutTheGame { get; set; }
        public string ShortDescription { get; set; }
        public string HeaderImage { get; set; }
        public string ReleaseDate { get; set; }
        //public bool Windows { get; set; }
        //public bool Mac { get; set; }
        //public bool Linux { get; set; }
        public virtual OrmRecommendations Recommendations { get; set; }
        public virtual OrmPriceOverview PriceOverview { get; set; }
        public virtual OrmMetacritic Metacritic { get; set; }
        public virtual OrmSupportInfo SupportInfo { get; set; }
        public virtual OrmBackground Background { get; set; }
        public virtual OrmPlatform Platforms { get; set; }
        public virtual ICollection<OrmRequirements> Requirements { get; set; }
        public virtual ICollection<OrmGameDeveloper> GameDevelopers { get; set; }
        public virtual ICollection<OrmGamePublisher> GamePublishers { get; set; }
        public virtual ICollection<OrmScreenshot> Screenshots { get; set; }
        public virtual ICollection<OrmMovie> Movies { get; set; }
        public virtual ICollection<OrmGameLanguage> GameLanguages { get; set; }
    }
    // One developer has multiple games - same dev different game
    public class OrmDeveloper
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<OrmGameDeveloper> GameDevelopers { get; set; }
    }
    public class OrmPlatform
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public bool Windows { get; set; }
        public bool Mac { get; set; }
        public bool Linux { get; set; }
        public virtual OrmGame OrmGame { get; set; }
    }

    // One publisher has multiple games - same publisher different games
    public class OrmPublisher
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<OrmGamePublisher> GamePublishers { get; set; }
    }
    // One GameDeveloper has one game and one developer
    public class OrmGameDeveloper
    {
        [Key, Column(Order = 0)]
        public int GameAppId { get; set; }

        [Key, Column(Order = 1)]
        public int DeveloperId { get; set; }

        public virtual OrmGame OrmGame { get; set; }
        public virtual OrmDeveloper OrmDeveloper { get; set; }
    }
    // One GamePublisher has one game and one publisher
    public class OrmGamePublisher
    {
        [Key, Column(Order = 0)]
        public int GameAppId { get; set; }

        [Key, Column(Order = 1)]
        public int PublisherId { get; set; }

        public virtual OrmGame OrmGame { get; set; }
        public virtual OrmPublisher OrmPublisher { get; set; }
    }
    // Requirement has one Game and Game has one Requirement
    public class OrmRequirements
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string Platform { get; set; }
        public string Minimum { get; set; }

        public virtual OrmGame OrmGame { get; set; }
    }
    // PriceOverview has one Game and Game has one PriceOverview
    public class OrmPriceOverview
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string Currency { get; set; }
        public int? DiscountPercent { get; set; }
        public string FinalFormatted { get; set; }

        public virtual OrmGame OrmGame { get; set; }
    }
    // Metacritic has one Game and Game has one Metacritic
    public class OrmMetacritic
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public int? Score { get; set; }
        public string Url { get; set; }

        public virtual OrmGame OrmGame { get; set; }
    }
    // Screenshot has one game - game has multiple screenshots
    [Table("Screenshot")]
    public class OrmScreenshot
    {
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string PathThumbnail { get; set; }
        public string PathFull { get; set; }
        [ForeignKey("GameAppId")]
        public OrmGame OrmGame { get; set; }
    }
    // Movie has one game - game has multiple movies
    [Table("Movie")]
    public class OrmMovie
    {
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public int MovieId { get; set; }
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public string Webm480 { get; set; }
        public string WebmMax { get; set; }
        public string Mp4480 { get; set; }
        public string Mp4Max { get; set; }
        public bool Highlight { get; set; }
        [ForeignKey("GameAppId")]
        public OrmGame OrmGame { get; set; }
    }
    //  Language has many game languages (same language different game) - game has many game languages (same game different languages)
    public class OrmLanguage
    {
        public int Id { get; set; }
        public string Language { get; set; }
        public virtual ICollection<OrmGameLanguage> GameLanguages { get; set; }
    }
    // Each game languages has one game and one language
    public class OrmGameLanguage
    {
        [Key]
        [Column(Order = 0)]
        public int GameAppId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int LanguageId { get; set; }

        [ForeignKey(nameof(GameAppId))]
        public OrmGame OrmGame { get; set; }

        [ForeignKey(nameof(LanguageId))]
        public OrmLanguage OrmLanguage { get; set; }
    }
    // Each recommendations has one game - game has one recommendations
    public class OrmRecommendations
    {
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string ReviewScoreDesc { get; set; }
        public int TotalReviews { get; set; }
        public int TotalPositive { get; set; }
        public int TotalNegative { get; set; }

        [ForeignKey(nameof(GameAppId))]
        public OrmGame OrmGame { get; set; }
    }
    // Each support info has one game and each game has one support info
    public class OrmSupportInfo
    {
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string Url { get; set; }
        public string Email { get; set; }

        [ForeignKey(nameof(GameAppId))]
        public OrmGame OrmGame { get; set; }
    }
    // Background has one game and game has one background
    public class OrmBackground
    {
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string Background { get; set; }
        public string BackgroundRaw { get; set; }

        [ForeignKey(nameof(GameAppId))]
        public OrmGame OrmGame { get; set; }
    }
    #endregion
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
        public int SteamAppId { get; set; }
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
        public ReleaseDate ReleaseDate { get; set; }

        [JsonProperty("support_info")]
        public SupportInfo SupportInfo { get; set; }

        [JsonProperty("background")]
        public string Background { get; set; }

        [JsonProperty("background_raw")]
        public string BackgroundRaw { get; set; }
    }
    public class PcRequirements
    {
        [JsonProperty("minimum")]
        public string Minimum { get; set; }
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


