﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SteamAppDetailsToSQL;

#nullable disable

namespace SteamAppToSQL.Migrations
{
    [DbContext(typeof(SteamDbContext))]
    partial class SteamDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoBackground", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Background")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BackgroundRaw")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("Backgrounds");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoDeveloper", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Developers");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoGame", b =>
                {
                    b.Property<int>("SteamAppId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SteamAppId"));

                    b.Property<string>("AboutTheGame")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DetailedDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HeaderImage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsFree")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReleaseDate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RequiredAge")
                        .HasColumnType("int");

                    b.Property<string>("ShortDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SteamAppId");

                    b.HasIndex("SteamAppId")
                        .IsUnique();

                    b.ToTable("Game");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoGameDeveloper", b =>
                {
                    b.Property<int>("GameAppId")
                        .HasColumnType("int")
                        .HasColumnOrder(0);

                    b.Property<int>("DeveloperId")
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    b.HasKey("GameAppId", "DeveloperId");

                    b.HasIndex("DeveloperId");

                    b.ToTable("GameDevelopers");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoGameLanguage", b =>
                {
                    b.Property<int>("GameAppId")
                        .HasColumnType("int")
                        .HasColumnOrder(0);

                    b.Property<int>("LanguageId")
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    b.HasKey("GameAppId", "LanguageId");

                    b.HasIndex("LanguageId");

                    b.ToTable("GameLanguages");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoGamePublisher", b =>
                {
                    b.Property<int>("GameAppId")
                        .HasColumnType("int")
                        .HasColumnOrder(0);

                    b.Property<int>("PublisherId")
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    b.HasKey("GameAppId", "PublisherId");

                    b.HasIndex("PublisherId");

                    b.ToTable("GamePublishers");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoLanguage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Language")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Language")
                        .IsUnique()
                        .HasFilter("[Language] IS NOT NULL");

                    b.ToTable("Language");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoMetacritic", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<int?>("Score")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("Metacritic");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoMovie", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<bool>("Highlight")
                        .HasColumnType("bit");

                    b.Property<string>("Mp4480")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Mp4Max")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Thumbnail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Webm480")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WebmMax")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId");

                    b.ToTable("Movie");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoPlatform", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<bool>("Linux")
                        .HasColumnType("bit");

                    b.Property<bool>("Mac")
                        .HasColumnType("bit");

                    b.Property<bool>("Windows")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("Platforms");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoPriceOverview", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Currency")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DiscountPercent")
                        .HasColumnType("int");

                    b.Property<string>("FinalFormatted")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("PriceOverview");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoPublisher", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Publishers");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoRecommendations", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<string>("ReviewScoreDesc")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TotalNegative")
                        .HasColumnType("int");

                    b.Property<int>("TotalPositive")
                        .HasColumnType("int");

                    b.Property<int>("TotalReviews")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("Recommendations");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoRequirements", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<string>("Minimum")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Platform")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId");

                    b.ToTable("Requirements");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoScreenshot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<string>("PathFull")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PathThumbnail")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId");

                    b.ToTable("Screenshot");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoSupportInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("SupportInfo");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoBackground", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithOne("Backgrounds")
                        .HasForeignKey("SteamAppDetailsToSQL.DtoBackground", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoGameDeveloper", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoDeveloper", "DtoDeveloper")
                        .WithMany("GameDevelopers")
                        .HasForeignKey("DeveloperId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithMany("GameDevelopers")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoDeveloper");

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoGameLanguage", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithMany("GameLanguages")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SteamAppDetailsToSQL.DtoLanguage", "DtoLanguage")
                        .WithMany("GameLanguages")
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");

                    b.Navigation("DtoLanguage");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoGamePublisher", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithMany("GamePublishers")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SteamAppDetailsToSQL.DtoPublisher", "DtoPublisher")
                        .WithMany("GamePublishers")
                        .HasForeignKey("PublisherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");

                    b.Navigation("DtoPublisher");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoMetacritic", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithOne("Metacritic")
                        .HasForeignKey("SteamAppDetailsToSQL.DtoMetacritic", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoMovie", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithMany("Movies")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoPlatform", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithOne("Platforms")
                        .HasForeignKey("SteamAppDetailsToSQL.DtoPlatform", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoPriceOverview", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithOne("PriceOverview")
                        .HasForeignKey("SteamAppDetailsToSQL.DtoPriceOverview", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoRecommendations", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithOne("Recommendations")
                        .HasForeignKey("SteamAppDetailsToSQL.DtoRecommendations", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoRequirements", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithMany("Requirements")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoScreenshot", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithMany("Screenshots")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoSupportInfo", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.DtoGame", "DtoGame")
                        .WithOne("SupportInfo")
                        .HasForeignKey("SteamAppDetailsToSQL.DtoSupportInfo", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DtoGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoDeveloper", b =>
                {
                    b.Navigation("GameDevelopers");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoGame", b =>
                {
                    b.Navigation("Backgrounds")
                        .IsRequired();

                    b.Navigation("GameDevelopers");

                    b.Navigation("GameLanguages");

                    b.Navigation("GamePublishers");

                    b.Navigation("Metacritic")
                        .IsRequired();

                    b.Navigation("Movies");

                    b.Navigation("Platforms")
                        .IsRequired();

                    b.Navigation("PriceOverview")
                        .IsRequired();

                    b.Navigation("Recommendations")
                        .IsRequired();

                    b.Navigation("Requirements");

                    b.Navigation("Screenshots");

                    b.Navigation("SupportInfo")
                        .IsRequired();
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoLanguage", b =>
                {
                    b.Navigation("GameLanguages");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.DtoPublisher", b =>
                {
                    b.Navigation("GamePublishers");
                });
#pragma warning restore 612, 618
        }
    }
}
