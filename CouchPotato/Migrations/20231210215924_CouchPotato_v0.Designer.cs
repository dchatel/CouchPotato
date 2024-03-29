﻿// <auto-generated />
using System;
using CouchPotato.DbModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CouchPotato.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20231210215924_CouchPotato_v0")]
    partial class CouchPotato_v0
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("NOCASE")
                .HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("CouchPotato.DbModel.Episode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DigitalFileFormat")
                        .HasColumnType("TEXT");

                    b.Property<string>("DigitalResolution")
                        .HasColumnType("TEXT");

                    b.Property<int>("EpisodeNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Overview")
                        .HasColumnType("TEXT");

                    b.Property<int?>("PersonalRating")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Runtime")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SeasonId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TmdbId")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("TmdbRating")
                        .HasColumnType("REAL");

                    b.Property<int?>("TmdbRatingCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SeasonId");

                    b.ToTable("Episode");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Genre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Fixed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Genre");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PortraitUrl")
                        .HasColumnType("TEXT");

                    b.Property<int?>("TmdbId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Person");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Characters")
                        .HasColumnType("TEXT");

                    b.Property<int>("Order")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PersonId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VideoId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.HasIndex("VideoId");

                    b.ToTable("Role");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Season", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Overview")
                        .HasColumnType("TEXT");

                    b.Property<string>("PosterUrl")
                        .HasColumnType("TEXT");

                    b.Property<int>("SeasonNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TVShowId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TmdbId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TVShowId");

                    b.ToTable("Season");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Video", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BackgroundUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("DigitalFileFormat")
                        .HasColumnType("TEXT");

                    b.Property<string>("DigitalResolution")
                        .HasColumnType("TEXT");

                    b.Property<string>("DigitalStorageCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Origin")
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalTitle")
                        .HasColumnType("TEXT");

                    b.Property<string>("Overview")
                        .HasColumnType("TEXT");

                    b.Property<int?>("PersonalRating")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PhysicalStorage")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhysicalStorageCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("PosterUrl")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Runtime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<string>("Tagline")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("TmdbId")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("TmdbRating")
                        .HasColumnType("REAL");

                    b.Property<int?>("TmdbRatingCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Version")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Video");
                });

            modelBuilder.Entity("GenreVideo", b =>
                {
                    b.Property<int>("GenresId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VideosId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GenresId", "VideosId");

                    b.HasIndex("VideosId");

                    b.ToTable("GenreVideo");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Episode", b =>
                {
                    b.HasOne("CouchPotato.DbModel.Season", "Season")
                        .WithMany("Episodes")
                        .HasForeignKey("SeasonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Season");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Role", b =>
                {
                    b.HasOne("CouchPotato.DbModel.Person", "Person")
                        .WithMany("Roles")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CouchPotato.DbModel.Video", "Video")
                        .WithMany("Roles")
                        .HasForeignKey("VideoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");

                    b.Navigation("Video");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Season", b =>
                {
                    b.HasOne("CouchPotato.DbModel.Video", "TVShow")
                        .WithMany("Seasons")
                        .HasForeignKey("TVShowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TVShow");
                });

            modelBuilder.Entity("GenreVideo", b =>
                {
                    b.HasOne("CouchPotato.DbModel.Genre", null)
                        .WithMany()
                        .HasForeignKey("GenresId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CouchPotato.DbModel.Video", null)
                        .WithMany()
                        .HasForeignKey("VideosId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CouchPotato.DbModel.Person", b =>
                {
                    b.Navigation("Roles");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Season", b =>
                {
                    b.Navigation("Episodes");
                });

            modelBuilder.Entity("CouchPotato.DbModel.Video", b =>
                {
                    b.Navigation("Roles");

                    b.Navigation("Seasons");
                });
#pragma warning restore 612, 618
        }
    }
}
