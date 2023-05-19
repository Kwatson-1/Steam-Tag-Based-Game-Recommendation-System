using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamAppToSQL.Migrations
{
    /// <inheritdoc />
    public partial class IncludePlatformTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Background_Game_GameAppId",
                table: "Background");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Background",
                table: "Background");

            migrationBuilder.DropColumn(
                name: "Linux",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "Mac",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "Windows",
                table: "Game");

            migrationBuilder.RenameTable(
                name: "Background",
                newName: "Backgrounds");

            migrationBuilder.RenameIndex(
                name: "IX_Background_GameAppId",
                table: "Backgrounds",
                newName: "IX_Backgrounds_GameAppId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Backgrounds",
                table: "Backgrounds",
                column: "Id");

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

            migrationBuilder.CreateIndex(
                name: "IX_Game_SteamAppId",
                table: "Game",
                column: "SteamAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_GameAppId",
                table: "Platforms",
                column: "GameAppId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Backgrounds_Game_GameAppId",
                table: "Backgrounds",
                column: "GameAppId",
                principalTable: "Game",
                principalColumn: "SteamAppId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Backgrounds_Game_GameAppId",
                table: "Backgrounds");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.DropIndex(
                name: "IX_Game_SteamAppId",
                table: "Game");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Backgrounds",
                table: "Backgrounds");

            migrationBuilder.RenameTable(
                name: "Backgrounds",
                newName: "Background");

            migrationBuilder.RenameIndex(
                name: "IX_Backgrounds_GameAppId",
                table: "Background",
                newName: "IX_Background_GameAppId");

            migrationBuilder.AddColumn<bool>(
                name: "Linux",
                table: "Game",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Mac",
                table: "Game",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Windows",
                table: "Game",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Background",
                table: "Background",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Background_Game_GameAppId",
                table: "Background",
                column: "GameAppId",
                principalTable: "Game",
                principalColumn: "SteamAppId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
