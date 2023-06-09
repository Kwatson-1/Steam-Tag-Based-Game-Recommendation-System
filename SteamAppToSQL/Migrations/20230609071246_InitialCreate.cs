using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamAppToSQL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Developers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Developers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    SteamAppId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredAge = table.Column<int>(type: "int", nullable: false),
                    IsFree = table.Column<bool>(type: "bit", nullable: false),
                    DetailedDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AboutTheGame = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleaseDate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game", x => x.SteamAppId);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Language = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Backgrounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    Background = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BackgroundRaw = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backgrounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Backgrounds_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameDevelopers",
                columns: table => new
                {
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    DeveloperId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameDevelopers", x => new { x.GameAppId, x.DeveloperId });
                    table.ForeignKey(
                        name: "FK_GameDevelopers_Developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Developers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameDevelopers_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Metacritic",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metacritic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metacritic_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Movie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Thumbnail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Webm480 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebmMax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mp4480 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mp4Max = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Highlight = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movie_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    Windows = table.Column<bool>(type: "bit", nullable: false),
                    Mac = table.Column<bool>(type: "bit", nullable: false),
                    Linux = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Platforms_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceOverview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    FinalFormatted = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceOverview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceOverview_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    ReviewScoreDesc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    TotalPositive = table.Column<int>(type: "int", nullable: false),
                    TotalNegative = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Minimum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requirements_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Screenshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    PathThumbnail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathFull = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screenshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Screenshot_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportInfo_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameLanguages",
                columns: table => new
                {
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLanguages", x => new { x.GameAppId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_GameLanguages_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameLanguages_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GamePublishers",
                columns: table => new
                {
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    PublisherId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePublishers", x => new { x.GameAppId, x.PublisherId });
                    table.ForeignKey(
                        name: "FK_GamePublishers_Game_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Game",
                        principalColumn: "SteamAppId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePublishers_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Backgrounds_GameAppId",
                table: "Backgrounds",
                column: "GameAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Developers_Name",
                table: "Developers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Game_SteamAppId",
                table: "Game",
                column: "SteamAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameDevelopers_DeveloperId",
                table: "GameDevelopers",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_GameLanguages_LanguageId",
                table: "GameLanguages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePublishers_PublisherId",
                table: "GamePublishers",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_Language_Language",
                table: "Language",
                column: "Language",
                unique: true,
                filter: "[Language] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Metacritic_GameAppId",
                table: "Metacritic",
                column: "GameAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movie_GameAppId",
                table: "Movie",
                column: "GameAppId");

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_GameAppId",
                table: "Platforms",
                column: "GameAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceOverview_GameAppId",
                table: "PriceOverview",
                column: "GameAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Publishers_Name",
                table: "Publishers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_GameAppId",
                table: "Recommendations",
                column: "GameAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_GameAppId",
                table: "Requirements",
                column: "GameAppId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenshot_GameAppId",
                table: "Screenshot",
                column: "GameAppId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportInfo_GameAppId",
                table: "SupportInfo",
                column: "GameAppId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Backgrounds");

            migrationBuilder.DropTable(
                name: "GameDevelopers");

            migrationBuilder.DropTable(
                name: "GameLanguages");

            migrationBuilder.DropTable(
                name: "GamePublishers");

            migrationBuilder.DropTable(
                name: "Metacritic");

            migrationBuilder.DropTable(
                name: "Movie");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.DropTable(
                name: "PriceOverview");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Requirements");

            migrationBuilder.DropTable(
                name: "Screenshot");

            migrationBuilder.DropTable(
                name: "SupportInfo");

            migrationBuilder.DropTable(
                name: "Developers");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "Publishers");

            migrationBuilder.DropTable(
                name: "Game");
        }
    }
}
