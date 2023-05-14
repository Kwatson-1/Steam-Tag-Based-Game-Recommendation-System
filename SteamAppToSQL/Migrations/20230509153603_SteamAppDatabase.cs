using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamAppToSQL.Migrations
{
    /// <inheritdoc />
    public partial class SteamAppDatabase : Migration
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Developers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    SteamAppId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredAge = table.Column<int>(type: "int", nullable: false),
                    IsFree = table.Column<bool>(type: "bit", nullable: false),
                    DetailedDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AboutTheGame = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeaderImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReleaseDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Windows = table.Column<bool>(type: "bit", nullable: false),
                    Mac = table.Column<bool>(type: "bit", nullable: false),
                    Linux = table.Column<bool>(type: "bit", nullable: false)
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
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Background",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    Background = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackgroundRaw = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Background", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Background_Game_GameAppId",
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
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Thumbnail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Webm480 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebmMax = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mp4480 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mp4Max = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "PriceOverview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameAppId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: true),
                    FinalFormatted = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    Platform = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Minimum = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    PathThumbnail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PathFull = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "IX_Background_GameAppId",
                table: "Background",
                column: "GameAppId",
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
                name: "IX_Metacritic_GameAppId",
                table: "Metacritic",
                column: "GameAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movie_GameAppId",
                table: "Movie",
                column: "GameAppId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceOverview_GameAppId",
                table: "PriceOverview",
                column: "GameAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_GameAppId",
                table: "Recommendations",
                column: "GameAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_GameAppId",
                table: "Requirements",
                column: "GameAppId",
                unique: true);

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
                name: "Background");

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
