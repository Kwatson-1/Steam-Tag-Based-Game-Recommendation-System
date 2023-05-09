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

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmBackground", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Background")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BackgroundRaw")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("Background");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmDeveloper", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Developers");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmGame", b =>
                {
                    b.Property<int>("SteamAppId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SteamAppId"));

                    b.Property<string>("AboutTheGame")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DetailedDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HeaderImage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsFree")
                        .HasColumnType("bit");

                    b.Property<bool>("Linux")
                        .HasColumnType("bit");

                    b.Property<bool>("Mac")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReleaseDate")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RequiredAge")
                        .HasColumnType("int");

                    b.Property<string>("ShortDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Windows")
                        .HasColumnType("bit");

                    b.HasKey("SteamAppId");

                    b.ToTable("Game");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmGameDeveloper", b =>
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

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmGameLanguage", b =>
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

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmGamePublisher", b =>
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

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmLanguage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Language");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmMetacritic", b =>
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
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("Metacritic");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmMovie", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<bool>("Highlight")
                        .HasColumnType("bit");

                    b.Property<int>("MovieId")
                        .HasColumnType("int");

                    b.Property<string>("Mp4480")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Mp4Max")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Thumbnail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Webm480")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WebmMax")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId");

                    b.ToTable("Movie");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmPriceOverview", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DiscountPercent")
                        .HasColumnType("int");

                    b.Property<string>("FinalFormatted")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("PriceOverview");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmPublisher", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Publishers");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmRecommendations", b =>
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

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmRequirements", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<string>("Minimum")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Platform")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("Requirements");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmScreenshot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<string>("PathFull")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PathThumbnail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId");

                    b.ToTable("Screenshot");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmSupportInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GameAppId")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameAppId")
                        .IsUnique();

                    b.ToTable("SupportInfo");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmBackground", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithOne("Background")
                        .HasForeignKey("SteamAppDetailsToSQL.OrmBackground", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmGameDeveloper", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmDeveloper", "OrmDeveloper")
                        .WithMany("GameDevelopers")
                        .HasForeignKey("DeveloperId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithMany("GameDevelopers")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmDeveloper");

                    b.Navigation("OrmGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmGameLanguage", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithMany("GameLanguages")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SteamAppDetailsToSQL.OrmLanguage", "OrmLanguage")
                        .WithMany("GameLanguages")
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");

                    b.Navigation("OrmLanguage");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmGamePublisher", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithMany("GamePublishers")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SteamAppDetailsToSQL.OrmPublisher", "OrmPublisher")
                        .WithMany("GamePublishers")
                        .HasForeignKey("PublisherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");

                    b.Navigation("OrmPublisher");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmMetacritic", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithOne("Metacritic")
                        .HasForeignKey("SteamAppDetailsToSQL.OrmMetacritic", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmMovie", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithMany("Movies")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmPriceOverview", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithOne("PriceOverview")
                        .HasForeignKey("SteamAppDetailsToSQL.OrmPriceOverview", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmRecommendations", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithOne("Recommendations")
                        .HasForeignKey("SteamAppDetailsToSQL.OrmRecommendations", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmRequirements", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithOne("Requirements")
                        .HasForeignKey("SteamAppDetailsToSQL.OrmRequirements", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmScreenshot", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithMany("Screenshots")
                        .HasForeignKey("GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmSupportInfo", b =>
                {
                    b.HasOne("SteamAppDetailsToSQL.OrmGame", "OrmGame")
                        .WithOne("SupportInfo")
                        .HasForeignKey("SteamAppDetailsToSQL.OrmSupportInfo", "GameAppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OrmGame");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmDeveloper", b =>
                {
                    b.Navigation("GameDevelopers");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmGame", b =>
                {
                    b.Navigation("Background")
                        .IsRequired();

                    b.Navigation("GameDevelopers");

                    b.Navigation("GameLanguages");

                    b.Navigation("GamePublishers");

                    b.Navigation("Metacritic")
                        .IsRequired();

                    b.Navigation("Movies");

                    b.Navigation("PriceOverview")
                        .IsRequired();

                    b.Navigation("Recommendations")
                        .IsRequired();

                    b.Navigation("Requirements")
                        .IsRequired();

                    b.Navigation("Screenshots");

                    b.Navigation("SupportInfo")
                        .IsRequired();
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmLanguage", b =>
                {
                    b.Navigation("GameLanguages");
                });

            modelBuilder.Entity("SteamAppDetailsToSQL.OrmPublisher", b =>
                {
                    b.Navigation("GamePublishers");
                });
#pragma warning restore 612, 618
        }
    }
}