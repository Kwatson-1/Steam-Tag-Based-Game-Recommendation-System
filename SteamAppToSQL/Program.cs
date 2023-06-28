using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
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
        static int skipCounter = 0;
        static int addCounter = 0;

        public static async Task Main(string[] args)
        {
            IConfiguration configuration = CreateConfiguration();
            AppSettings appSettings = new()
            {
                SteamAppDatabase = configuration["SteamAppDatabase"],
                SteamWebApi = configuration["SteamWebApi"]
            };

            var optionsBuilder = new DbContextOptionsBuilder<SteamDbContext>()

            .UseSqlServer(appSettings.SteamAppDatabase);

            //Testing for individual app
            //int appId = 1844580; // Replace with the desired app ID
            //var details = await FetchAppDetailsAsync(appId);
            //var reviews = await FetchAppReviewsAsync(appId);
            //MergedData md = new(details, reviews);

            List<SteamApp> steamApps = GetSteamAppsFromCsv();

            using (var dbContext = new SteamDbContext(optionsBuilder.Options, appSettings))
            {
                int operationCounter = 0;
                // Stopwatch for limiting api request rates
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                foreach (var app in steamApps)
                {
                    try
                    {
                        dbContext.ChangeTracker.Clear();

                        var md = await GetAppData(app);
                        operationCounter++;

                        if (operationCounter >= 125)
                        {
                            stopwatch.Stop();

                            if (stopwatch.Elapsed < TimeSpan.FromMinutes(5))
                            {
                                // Calculate remaining time to wait
                                var remainingTime = TimeSpan.FromMinutes(5) - stopwatch.Elapsed;

                                Console.WriteLine($"Delaying for {remainingTime.TotalSeconds} seconds...");
                                await Task.Delay(remainingTime);
                            }

                            // Reset the operation counter and stopwatch
                            operationCounter = 0;
                            stopwatch.Reset();
                            stopwatch.Start();
                        }

                        if (md == null)
                        {
                            continue;
                        }

                        var dto = MapMergedDataToDto(md, dbContext);
                        dbContext.Add(dto);
                        await dbContext.SaveChangesAsync();
                        addCounter++;
                        Console.WriteLine($"{addCounter}/{addCounter + skipCounter}. {app.Name}({app.AppId}): added to the database...");
                    }
                    catch (Exception ex)
                    {
                        skipCounter++;
                        Console.WriteLine($"Failure #{skipCounter}. {app.Name}({app.AppId}): {ex.Message}\n");
                        dbContext.ChangeTracker.Clear();
                        continue;
                    }
                }
            }
            Console.ReadLine();
        }
        #region Deprecated
        public static async Task WriteBatchesToFile(List<SteamApp> appList, string fileName, int batchSize = 2)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(currentDirectory, fileName);

            using var streamWriter = new StreamWriter(filePath, append: true);

            // If the file is empty, write the opening bracket of a JSON array.
            if (new FileInfo(filePath).Length == 0)
            {
                await streamWriter.WriteAsync("[");
            }

            // Iterate through the app list in batches.
            for (int i = 0; i < appList.Count; i += batchSize)
            {
                Console.WriteLine($"Processing batch {(i / batchSize) + 1} of {Math.Ceiling((double)appList.Count / batchSize)}...");

                var batch = appList.Skip(i).Take(batchSize).ToList();

                // Fetch data for each app in the batch sequentially.
                Console.WriteLine("Fetching data for apps in current batch...");
                List<MergedData> results = new List<MergedData>();
                foreach (var app in batch)
                {
                    var result = await GetAppData(app);
                    if (result != null)
                    {
                        results.Add(result);
                    }
                }

                // Serialize the valid results to a JSON string.
                var jsonString = JsonConvert.SerializeObject(results);

                // If this is not the first batch, prepend a comma to the JSON string.
                if (i > 0)
                {
                    jsonString = "," + jsonString;
                }

                // Remove the opening and closing brackets of the JSON array.
                jsonString = jsonString.Substring(1, jsonString.Length - 2);

                // Write the JSON string to the file.
                await streamWriter.WriteAsync(jsonString);
                Console.WriteLine("Data for current batch saved to file...");

                // If this is not the last batch, wait for 2 minutes before proceeding to the next batch.
                if (i + batchSize < appList.Count)
                {
                    Console.WriteLine("Waiting for 2 minutes before fetching the next batch...");
                    await Task.Delay(TimeSpan.FromMinutes(2));
                }
            }

            // Write the closing bracket of the JSON array.
            await streamWriter.WriteAsync("]");
            Console.WriteLine($"All data saved to {filePath}");
        }

        public static async Task<List<MergedData>> ReadDataFromFile(string fileName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(currentDirectory, fileName);

            Console.WriteLine($"Reading data from {filePath}...");

            // Read the file as a JSON string.
            string jsonString = await File.ReadAllTextAsync(filePath);

            // Deserialize the JSON string to a list of MergedData objects.
            List<MergedData> dataList = JsonConvert.DeserializeObject<List<MergedData>>(jsonString);

            Console.WriteLine($"Deserialized {dataList.Count} MergedData objects from {fileName}");

            return dataList;
        }

        public static void WriteObjectToFile(MergedData md, string fileName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(currentDirectory, fileName);

            var jsonString = JsonConvert.SerializeObject(md);
            File.AppendAllText(filePath, jsonString + Environment.NewLine); // appending JSON string to the existing file

            Console.WriteLine($"Object saved to {filePath}");
        }
        #endregion
        #region App List Methods
        // Utility method to check if a string is in English
        public static bool IsEnglish(string input)
        {
            // Regular expression pattern for matching English letters, digits, spaces, and common punctuation
            string pattern = @"^[a-zA-Z0-9 .,;:'""!?@#$%^&*()-={}|<>]*$";

            // Use regular expressions to check if the input matches the pattern
            return Regex.IsMatch(input, pattern);
        }
        // Gets the steam app list, updates the output file and returns the new List<SteamApp> object
        public static async Task<List<SteamApp>> UpdateGetAppList()
        {
            List<SteamApp> apps = await GetSteamAppsAsync();
            string outputPath = "steam_apps.csv";
            SaveSteamAppsToCsv(apps, outputPath);
            Console.WriteLine($"Saved {apps.Count} apps to {outputPath}");
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

            // Filter the appList to include only English app names
            List<SteamApp> englishApps = appList.Where(app => IsEnglish(app.Name)).ToList();

            return englishApps;
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
            using (StreamReader sr = new("steam_apps.csv"))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] values = line.Split(',');

                    if (values.Length < 2)
                    {
                        continue; // Skip the line if it does not have at least two values.
                    }

                    if (int.TryParse(values[0], out int appId))
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

            string jsonDetailsResponseInner = ExtractInnerJson(jsonDetailsResponse); // If unable to ExtractInnerJson will return null

            if (jsonDetailsResponseInner == null)
            {
                Console.WriteLine($"{appId} has no inner JSON to deserialize.");
                skipCounter++;
                return null;
            }
            // Deserialize the JSON response
            try
            {
                DetailsRoot myDeserializedDetailsClass = JsonConvert.DeserializeObject<DetailsRoot>(jsonDetailsResponseInner) ?? throw new Exception("There was no data to deserialize.");

                return myDeserializedDetailsClass;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{appId} encountered error: {ex.Message}");
                skipCounter++;
                return null;
            }
        }
        // Returns a deserialized response from the Steam app reviews API
        public static async Task<ReviewRoot> FetchAppReviewsAsync(int appId)
        {
            string steamAppReviewUrl = $"https://store.steampowered.com/appreviews/{appId}?json=1";

            using HttpClient httpClient = new();

            HttpResponseMessage reviewResponse = await httpClient.GetAsync(steamAppReviewUrl);
            string jsonReviewResponse = await reviewResponse.Content.ReadAsStringAsync();

            // Deserialize the JSON response
            try
            {
                ReviewRoot myDeserializedReviewClass = JsonConvert.DeserializeObject<ReviewRoot>(jsonReviewResponse) ?? throw new Exception("There was no data to deserialize.");
                return myDeserializedReviewClass;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{appId} encountered error: {ex.Message}");
                skipCounter++;
                return null;
            }
        }
        public static string ExtractInnerJson(string jsonString)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }


        }
        #endregion
        // Suppresses missing IHost builder message in PowerShell
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args);
        }
        // Use to retrieve all app data and return a list of MergedData - takes a List<SteamApp>
        public static async Task<MergedData?> GetAppData(SteamApp app)
        {
            var details = await FetchAppDetailsAsync(app.AppId);

            if(details == null)
            {
                return null;
            }
            // If details content is null, return
            if (!details.Success || details.Data == null)
            {
                return null;
            }

            var reviews = await FetchAppReviewsAsync(app.AppId);

            if(reviews == null)
            {
                return null;
            }
            // If review content is null, return
            if (reviews.Success == 0 || reviews.QuerySummary == null)
            {
                return null;
            }

            return new MergedData(details, reviews);
        }

        public static List<MergedData> ReadObjectsFromFile(string fileName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(currentDirectory, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var jsonString = File.ReadAllText(filePath);
            var objects = JsonConvert.DeserializeObject<List<MergedData>>(jsonString);

            return objects;
        }
        #region MergedData to Dto Method
        public static DtoGame MapMergedDataToDto(MergedData mergedData, SteamDbContext steamDbContext)
        {
            // Relative path shortcuts
            var reviewsData = mergedData.ReviewObject.QuerySummary;
            var detailsData = mergedData.DetailsObject.Data;

            DtoGame dtoGame = new();

            // Game
            if (detailsData != null)
            {
                dtoGame.SteamAppId = detailsData.SteamAppId;
                dtoGame.Type = detailsData.Type;
                dtoGame.Name = detailsData.Name;
                dtoGame.RequiredAge = detailsData.RequiredAge;
                dtoGame.IsFree = detailsData.IsFree;
                dtoGame.DetailedDescription = detailsData.DetailedDescription;
                dtoGame.AboutTheGame = detailsData.AboutTheGame;
                dtoGame.ShortDescription = detailsData.ShortDescription;
                dtoGame.HeaderImage = detailsData.HeaderImage;
                dtoGame.ReleaseDate = detailsData.ReleaseDate.Date;
            }

            // Platform
            if (detailsData.Platforms != null)
            {
                dtoGame.Platforms = new DtoPlatform
                {
                    DtoGame = dtoGame,
                    Windows = detailsData.Platforms.Windows,
                    Mac = detailsData.Platforms.Mac,
                    Linux = detailsData.Platforms.Linux
                };
            }

            // Support Info
            if (detailsData.SupportInfo != null)
            {
                dtoGame.SupportInfo = new DtoSupportInfo
                {
                    DtoGame = dtoGame,
                    Url = detailsData.SupportInfo.Url,
                    Email = detailsData.SupportInfo.Email
                };
            }

            // Price Overview
            if (detailsData.PriceOverview != null)
            {
                dtoGame.PriceOverview = new DtoPriceOverview
                {
                    DtoGame = dtoGame,
                    Currency = detailsData.PriceOverview.Currency,
                    DiscountPercent = detailsData.PriceOverview.DiscountPercent,
                    FinalFormatted = detailsData.PriceOverview.FinalFormatted
                };
            }

            // Recommendations
            if (detailsData.Recommendations != null)
            {
                dtoGame.Recommendations = new DtoRecommendations
                {
                    DtoGame = dtoGame,
                    ReviewScoreDesc = reviewsData.ReviewScoreDesc,
                    TotalReviews = reviewsData.TotalReviews,
                    TotalPositive = reviewsData.TotalPositive,
                    TotalNegative = reviewsData.TotalNegative
                };
            }

            // Metacritic
            if (detailsData.Metacritic != null)
            {
                dtoGame.Metacritic = new DtoMetacritic
                {
                    DtoGame = dtoGame,
                    Score = detailsData.Metacritic.Score,
                    Url = detailsData.Metacritic.Url
                };
            }

            if (detailsData.Background != null && detailsData.BackgroundRaw != null)
            {
                // Backgrounds
                dtoGame.Backgrounds = new DtoBackground
                {
                    DtoGame = dtoGame,
                    Background = detailsData.Background,
                    BackgroundRaw = detailsData.BackgroundRaw
                };
            }

            // Developers and GameDevelopers
            dtoGame.GameDevelopers = new List<DtoGameDeveloper>();
            if (detailsData.Developers != null)
            {
                foreach (var developer in detailsData.Developers)
                {
                    // Check if developer exists in the database
                    var dtoDeveloper = steamDbContext.Developers.FirstOrDefault(d => d.Name == developer);

                    // If developer does not exist, create a new one
                    if (dtoDeveloper == null)
                    {
                        dtoDeveloper = new DtoDeveloper { Name = developer };
                        steamDbContext.Developers.Add(dtoDeveloper);
                    }

                    dtoGame.GameDevelopers.Add(new DtoGameDeveloper
                    {
                        DtoGame = dtoGame,
                        DtoDeveloper = dtoDeveloper
                    });
                }
                //steamDbContext.SaveChanges();
            }

            // Publishers and Game Publishers
            if (detailsData.Publishers != null)
            {
                dtoGame.GamePublishers = new List<DtoGamePublisher>();
                foreach (var publisher in detailsData.Publishers)
                {
                    // Check if publisher exists in the database
                    var dtoPublisher = steamDbContext.Publishers.FirstOrDefault(p => p.Name == publisher);

                    // If publisher does not exist, create a new one
                    if (dtoPublisher == null)
                    {
                        dtoPublisher = new DtoPublisher { Name = publisher };
                        steamDbContext.Publishers.Add(dtoPublisher);
                    }

                    dtoGame.GamePublishers.Add(new DtoGamePublisher
                    {
                        DtoGame = dtoGame,
                        DtoPublisher = dtoPublisher
                    });
                }
            }

            // Languages and Game Languages
            if (detailsData.SupportedLanguages != null)
            {
                dtoGame.GameLanguages = new List<DtoGameLanguage>();
                // Splits language string by comma delimeter and trims whitespace
                var languages = detailsData.SupportedLanguages.Split(',')
                                                               .Select(language => language.Trim())
                                                               .ToArray();

                foreach (var language in languages)
                {
                    // Check if language exists in the database
                    var dtoLanguage = steamDbContext.Language.FirstOrDefault(l => l.Language == language);

                    // If language does not exist, create a new one
                    if (dtoLanguage == null)
                    {
                        dtoLanguage = new DtoLanguage { Language = language };
                        steamDbContext.Language.Add(dtoLanguage);
                    }

                    dtoGame.GameLanguages.Add(new DtoGameLanguage
                    {
                        DtoGame = dtoGame,
                        DtoLanguage = dtoLanguage
                    });
                }
            }

            // Requirements
            if (detailsData.PcRequirements != null || detailsData.LinuxRequirements != null || detailsData.MacRequirements != null)
            {
                dtoGame.Requirements = new List<DtoRequirements>();

                if (detailsData.LinuxRequirements != null)
                {
                    dtoGame.Requirements.Add(new DtoRequirements
                    {
                        DtoGame = dtoGame,
                        Platform = "Linux",
                        Minimum = detailsData.LinuxRequirements.Minimum
                    });
                }
                if (detailsData.PcRequirements != null)
                {
                    dtoGame.Requirements.Add(new DtoRequirements
                    {
                        DtoGame = dtoGame,
                        Platform = "Windows",
                        Minimum = detailsData.PcRequirements.Minimum
                    });
                }
                if (detailsData.MacRequirements != null)
                {
                    dtoGame.Requirements.Add(new DtoRequirements
                    {
                        DtoGame = dtoGame,
                        Platform = "Mac",
                        Minimum = detailsData.MacRequirements.Minimum
                    });
                }
            }
            // Movies
            if (detailsData.Movies != null)
            {
                dtoGame.Movies = new List<DtoMovie>();

                foreach (var movie in detailsData.Movies)
                {
                    var dtoMovie = new DtoMovie
                    {
                        DtoGame = dtoGame,
                        //MovieId = movie.Id,
                        Name = movie.Name,
                        Thumbnail = movie.Thumbnail,
                        Webm480 = movie.Webm?._480,
                        WebmMax = movie.Webm?.Max,
                        Mp4480 = movie.Mp4?._480,
                        Mp4Max = movie.Mp4?.Max,
                        Highlight = movie.Highlight
                    };
                    dtoGame.Movies.Add(dtoMovie);
                }
            }
            // Screenshots
            if (detailsData.Screenshots != null)
            {
                dtoGame.Screenshots = new List<DtoScreenshot>();
                foreach (var screenshot in detailsData.Screenshots)
                {
                    dtoGame.Screenshots.Add(new DtoScreenshot
                    {
                        DtoGame = dtoGame,
                        PathThumbnail = screenshot.PathThumbnail,
                        PathFull = screenshot.PathFull
                    });
                }
            }
            return dtoGame;
        }
        #endregion

        public static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
        }
    }

    #region App settings
    public class AppSettings
    {
        public string SteamAppDatabase { get; set; }
        public string SteamWebApi { get; set; }
        public AppSettings()
        {

        }
        // Overloaded constructor
        public AppSettings(string steamAppDatabase, string steamWebApi)
        {
            SteamAppDatabase = steamAppDatabase;
            SteamWebApi = steamWebApi;
        }

    }
    #endregion
    #region Design Time DbContext Factory
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SteamDbContext>
    {
        public SteamDbContext CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<SteamDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("SteamAppDatabase"));

            return new SteamDbContext(optionsBuilder.Options, new AppSettings
            {
                SteamAppDatabase = configuration["SteamAppDatabase"],
                SteamWebApi = configuration["SteamWebApi"]
            });
        }
    }
    #endregion
    #region SteamDbContext
    // Creates a DbContext class that represents the database
    public sealed class SteamDbContext : DbContext
    {
        private readonly AppSettings _appSettings;

        public SteamDbContext(DbContextOptions<SteamDbContext> options, AppSettings appSettings)
            : base(options)
        {
            _appSettings = appSettings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = _appSettings.SteamAppDatabase;
            optionsBuilder.UseSqlServer(connectionString);
        }

        // DbSets entities here
        public DbSet<DtoGame> Game { get; set; }
        public DbSet<DtoDeveloper> Developers { get; set; }
        public DbSet<DtoPublisher> Publishers { get; set; }
        public DbSet<DtoGameDeveloper> GameDevelopers { get; set; }
        public DbSet<DtoGamePublisher> GamePublishers { get; set; }
        public DbSet<DtoRequirements> Requirements { get; set; }
        public DbSet<DtoPlatform> Platforms { get; set; }
        public DbSet<DtoPriceOverview> PriceOverview { get; set; }
        public DbSet<DtoMetacritic> Metacritic { get; set; }
        public DbSet<DtoScreenshot> Screenshots { get; set; }
        public DbSet<DtoMovie> Movies { get; set; }
        public DbSet<DtoRecommendations> Recommendations { get; set; }
        public DbSet<DtoSupportInfo> SupportInfo { get; set; }
        public DbSet<DtoBackground> Backgrounds { get; set; }
        public DbSet<DtoLanguage> Language { get; set; }
        public DbSet<DtoGameLanguage> GameLanguages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Game
            modelBuilder.Entity<DtoGame>()
                .Property(g => g.SteamAppId)
                .ValueGeneratedNever();

            modelBuilder.Entity<DtoGame>()
                .HasIndex(g => g.SteamAppId)
                .IsUnique();

            // Game -> GameDev
            modelBuilder.Entity<DtoGame>()
                .HasMany(g => g.GameDevelopers)
                .WithOne(gd => gd.DtoGame)
                .HasForeignKey(gd => gd.GameAppId);

            // Game -> GamePub
            modelBuilder.Entity<DtoGame>()
                .HasMany(g => g.GamePublishers)
                .WithOne(gp => gp.DtoGame)
                .HasForeignKey(gp => gp.GameAppId);

            // Game -> GameLang
            modelBuilder.Entity<DtoGame>()
                .HasMany(g => g.GameLanguages)
                .WithOne(gl => gl.DtoGame)
                .HasForeignKey(gl => gl.GameAppId);
            // Game Dev
            modelBuilder.Entity<DtoGameDeveloper>()
                .HasKey(gd => new { gd.GameAppId, gd.DeveloperId });

            // Game Publisher
            modelBuilder.Entity<DtoGamePublisher>()
                .HasKey(gp => new { gp.GameAppId, gp.PublisherId });

            // Game Lang
            modelBuilder.Entity<DtoGameLanguage>()
                .HasKey(gl => new { gl.GameAppId, gl.LanguageId });

            // Requirements
            modelBuilder.Entity<DtoRequirements>()
                .HasOne(r => r.DtoGame)
                .WithMany(g => g.Requirements)
                .HasForeignKey(r => r.GameAppId);

            // Platform
            modelBuilder.Entity<DtoPlatform>()
                .HasOne(m => m.DtoGame)
                .WithOne(g => g.Platforms)
                .HasForeignKey<DtoPlatform>(m => m.GameAppId);

            // Price Overview
            modelBuilder.Entity<DtoPriceOverview>()
                .HasOne(p => p.DtoGame)
                .WithOne(g => g.PriceOverview)
                .HasForeignKey<DtoPriceOverview>(p => p.GameAppId);

            // Metacritic
            modelBuilder.Entity<DtoMetacritic>()
                .HasOne(m => m.DtoGame)
                .WithOne(g => g.Metacritic)
                .HasForeignKey<DtoMetacritic>(m => m.GameAppId);

            // Screenshot
            modelBuilder.Entity<DtoScreenshot>()
                .HasOne(s => s.DtoGame)
                .WithMany(g => g.Screenshots)
                .HasForeignKey(s => s.GameAppId);

            // Movie
            modelBuilder.Entity<DtoMovie>()
                .HasOne(m => m.DtoGame)
                .WithMany(g => g.Movies)
                .HasForeignKey(m => m.GameAppId);

            // Recommendations
            modelBuilder.Entity<DtoRecommendations>()
                .HasOne(r => r.DtoGame)
                .WithOne(g => g.Recommendations)
                .HasForeignKey<DtoRecommendations>(r => r.GameAppId);

            // SupportInfo
            modelBuilder.Entity<DtoSupportInfo>()
                .HasOne(s => s.DtoGame)
                .WithOne(g => g.SupportInfo)
                .HasForeignKey<DtoSupportInfo>(s => s.GameAppId);

            // Background
            modelBuilder.Entity<DtoBackground>()
                .HasOne(b => b.DtoGame)
                .WithOne(g => g.Backgrounds)
                .HasForeignKey<DtoBackground>(b => b.GameAppId);

            // Developer
            modelBuilder.Entity<DtoDeveloper>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<DtoDeveloper>()
                .HasMany(d => d.GameDevelopers)
                .WithOne(gd => gd.DtoDeveloper)
                .HasForeignKey(gd => gd.DeveloperId);

            // Publisher
            modelBuilder.Entity<DtoPublisher>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<DtoPublisher>()
                .HasMany(p => p.GamePublishers)
                .WithOne(gp => gp.DtoPublisher)
                .HasForeignKey(gp => gp.PublisherId);

            // Language
            modelBuilder.Entity<DtoLanguage>()
                .HasIndex(l => l.Language)
                .IsUnique();

            modelBuilder.Entity<DtoLanguage>()
                .HasMany(l => l.GameLanguages)
                .WithOne(g => g.DtoLanguage)
                .HasForeignKey(l => l.LanguageId);
        }
    }
    #endregion
    #region DTO classes
    [Table("Game")]
    public class DtoGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SteamAppId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public int RequiredAge { get; set; }
        public bool IsFree { get; set; }
        public string? DetailedDescription { get; set; }
        public string? AboutTheGame { get; set; }
        public string? ShortDescription { get; set; }
        public string? HeaderImage { get; set; }
        public string? ReleaseDate { get; set; } = "Coming Soon";
        public virtual DtoRecommendations Recommendations { get; set; } = new DtoRecommendations();
        public virtual DtoPriceOverview PriceOverview { get; set; } = new DtoPriceOverview();
        public virtual DtoMetacritic Metacritic { get; set; } = new DtoMetacritic();
        public virtual DtoSupportInfo SupportInfo { get; set; } = new DtoSupportInfo();
        public virtual DtoBackground Backgrounds { get; set; } = new DtoBackground();
        public virtual DtoPlatform Platforms { get; set; } = new DtoPlatform();
        public virtual ICollection<DtoRequirements> Requirements { get; set; } = new List<DtoRequirements>();
        public virtual ICollection<DtoGameDeveloper> GameDevelopers { get; set; } = new List<DtoGameDeveloper>();
        public virtual ICollection<DtoGamePublisher> GamePublishers { get; set; } = new List<DtoGamePublisher>();
        public virtual ICollection<DtoScreenshot> Screenshots { get; set; } = new List<DtoScreenshot>();
        public virtual ICollection<DtoMovie> Movies { get; set; } = new List<DtoMovie>();
        public virtual ICollection<DtoGameLanguage> GameLanguages { get; set; } = new List<DtoGameLanguage>();
    }
    // One developer has multiple games - same dev different game
    public class DtoDeveloper
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<DtoGameDeveloper> GameDevelopers { get; set; }
    }
    public class DtoPlatform
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public bool Windows { get; set; }
        public bool Mac { get; set; }
        public bool Linux { get; set; }
        public virtual DtoGame DtoGame { get; set; }
    }

    // One publisher has multiple games - same publisher different games
    public class DtoPublisher
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<DtoGamePublisher> GamePublishers { get; set; }
    }
    // One GameDeveloper has one game and one developer
    public class DtoGameDeveloper
    {
        [Key, Column(Order = 0)]
        public int GameAppId { get; set; }
        [Key, Column(Order = 1)]
        public int DeveloperId { get; set; }
        public virtual DtoGame DtoGame { get; set; }
        public virtual DtoDeveloper DtoDeveloper { get; set; }
    }
    // One GamePublisher has one game and one publisher
    public class DtoGamePublisher
    {
        [Key, Column(Order = 0)]
        public int GameAppId { get; set; }
        [Key, Column(Order = 1)]
        public int PublisherId { get; set; }
        public virtual DtoGame DtoGame { get; set; }
        public virtual DtoPublisher DtoPublisher { get; set; }
    }
    // Requirement has one Game and Game has one Requirement
    public class DtoRequirements
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string? Platform { get; set; }
        public string? Minimum { get; set; } = string.Empty;
        public virtual DtoGame DtoGame { get; set; }
    }
    // PriceOverview has one Game and Game has one PriceOverview
    public class DtoPriceOverview
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string? Currency { get; set; }
        public int DiscountPercent { get; set; }
        public string? FinalFormatted { get; set; }
        public virtual DtoGame DtoGame { get; set; }
    }
    // Metacritic has one Game and Game has one Metacritic
    public class DtoMetacritic
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public int? Score { get; set; }
        public string? Url { get; set; }
        public virtual DtoGame DtoGame { get; set; }
    }
    // Screenshot has one game - game has multiple screenshots
    [Table("Screenshot")]
    public class DtoScreenshot
    {
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string? PathThumbnail { get; set; }
        public string? PathFull { get; set; }
        [ForeignKey("GameAppId")]
        public DtoGame DtoGame { get; set; }
    }
    // Movie has one game - game has multiple movies
    [Table("Movie")]
    public class DtoMovie
    {
        public int Id { get; set; }
        public int GameAppId { get; set; }
        //public int MovieId { get; set; }
        public string? Name { get; set; }
        public string? Thumbnail { get; set; }
        public string? Webm480 { get; set; }
        public string? WebmMax { get; set; }
        public string? Mp4480 { get; set; }
        public string? Mp4Max { get; set; }
        public bool Highlight { get; set; }
        [ForeignKey("GameAppId")]
        public DtoGame? DtoGame { get; set; }
    }
    //  Language has many game languages (same language different game) - game has many game languages (same game different languages)
    public class DtoLanguage
    {
        [Key]
        public int Id { get; set; }
        public string? Language { get; set; }
        public virtual ICollection<DtoGameLanguage> GameLanguages { get; set; }
    }
    // Each game languages has one game and one language
    public class DtoGameLanguage
    {
        [Key]
        [Column(Order = 0)]
        public int GameAppId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int LanguageId { get; set; }

        [ForeignKey(nameof(GameAppId))]
        public DtoGame DtoGame { get; set; }

        [ForeignKey(nameof(LanguageId))]
        public DtoLanguage DtoLanguage { get; set; }
    }
    // Each recommendations has one game - game has one recommendations
    public class DtoRecommendations
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string ReviewScoreDesc { get; set; } = "No user reviews";
        public int TotalReviews { get; set; }
        public int TotalPositive { get; set; }
        public int TotalNegative { get; set; }

        [ForeignKey(nameof(GameAppId))]
        public DtoGame DtoGame { get; set; }
    }
    // Each support info has one game and each game has one support info
    public class DtoSupportInfo
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string? Url { get; set; }
        public string? Email { get; set; }

        [ForeignKey(nameof(GameAppId))]
        public DtoGame DtoGame { get; set; }
    }
    // Background has one game and game has one background
    public class DtoBackground
    {
        [Key]
        public int Id { get; set; }
        public int GameAppId { get; set; }
        public string? Background { get; set; }
        public string? BackgroundRaw { get; set; }

        [ForeignKey(nameof(GameAppId))]
        public DtoGame DtoGame { get; set; }
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
        public DetailsRoot DetailsObject { get; set; }

        public ReviewRoot ReviewObject { get; set; }
        //public MergedData() { }
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
        public QuerySummary QuerySummary { get; set; }
    }
    public class QuerySummary
    {
        [JsonProperty("review_score_desc")]
        public string ReviewScoreDesc { get; set; } = "No user reviews";

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
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("steam_appid")]
        public int SteamAppId { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("required_age")]
        public int RequiredAge { get; set; }

        [JsonProperty("is_free")]
        public bool IsFree { get; set; }

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
        [JsonConverter(typeof(RequirementsConverter))]
        public PcRequirements PcRequirements { get; set; }

        [JsonProperty("mac_requirements")]
        [JsonConverter(typeof(RequirementsConverter))]
        public MacRequirements MacRequirements { get; set; }

        [JsonProperty("linux_requirements")]
        [JsonConverter(typeof(RequirementsConverter))]
        public LinuxRequirements LinuxRequirements { get; set; }

        [JsonProperty("developers")]
        public List<string> Developers { get; set; } = new List<string>();

        [JsonProperty("publishers")]
        public List<string> Publishers { get; set; } = new List<string>();

        [JsonProperty("price_overview")]
        public PriceOverview? PriceOverview { get; set; } = new PriceOverview();

        [JsonProperty("platforms")]
        public Platforms Platforms { get; set; } = new Platforms();

        [JsonProperty("metacritic")]
        public Metacritic? Metacritic { get; set; } = new Metacritic();

        [JsonProperty("screenshots")]
        public List<Screenshot> Screenshots { get; set; } = new List<Screenshot>();

        [JsonProperty("movies")]
        public List<Movie> Movies { get; set; } = new List<Movie>();

        [JsonProperty("recommendations")]
        public Recommendations Recommendations { get; set; } = new Recommendations();

        [JsonProperty("release_date")]
        public ReleaseDate ReleaseDate { get; set; } = new ReleaseDate();

        [JsonProperty("support_info")]
        public SupportInfo SupportInfo { get; set; } = new SupportInfo();

        [JsonProperty("background")]
        public string? Background { get; set; }

        [JsonProperty("background_raw")]
        public string? BackgroundRaw { get; set; }
    }
    /* 
     * When a game does not support a platform rather than being stored as an empty object, it is stored as an empty array which
     * confuses the deserializer due to an incompatible format. This method will return a null object when an empty array is encountered. 
    */
    public class RequirementsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Requirements).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // If the current token is an array start, return a new Requirements object.
            if (reader.TokenType == JsonToken.StartArray)
            {
                reader.Skip(); // Skip the array.
                return Activator.CreateInstance(objectType);
            }

            // If the current token is not an array, return the deserialized Requirements object.
            var target = Activator.CreateInstance(objectType);
            serializer.Populate(reader, target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Cast the value to Requirements
            var requirements = value as Requirements;

            // Begin a new JSON object
            writer.WriteStartObject();

            // Write the Minimum property
            writer.WritePropertyName("minimum");
            serializer.Serialize(writer, requirements.Minimum);

            // Write the Recommended property
            writer.WritePropertyName("recommended");
            serializer.Serialize(writer, requirements.Recommended);

            // End the object
            writer.WriteEndObject();
        }
    }

    public class PcRequirements : Requirements { }
    public class MacRequirements : Requirements { }
    public class LinuxRequirements : Requirements { }
    public class Requirements
    {
        [JsonProperty("minimum")]
        public string Minimum { get; set; } = string.Empty;
        public string Recommended { get; set; } = string.Empty;
    }

    public class Metacritic
    {
        [JsonProperty("score")]
        public int Score { get; set; }

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
    public class Webm
    {
        [JsonProperty("480")]
        public string? _480 { get; set; }

        [JsonProperty("max")]
        public string? Max { get; set; }
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
    #endregion

}


