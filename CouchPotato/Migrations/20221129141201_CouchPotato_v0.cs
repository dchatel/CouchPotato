using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CouchPotato.Migrations
{
    /// <inheritdoc />
    public partial class CouchPotatov0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Fixed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PortraitUrl = table.Column<string>(type: "TEXT", nullable: true),
                    TmdbId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Tagline = table.Column<string>(type: "TEXT", nullable: true),
                    Overview = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    Origin = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalTitle = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    PhysicalStorage = table.Column<string>(type: "TEXT", nullable: true),
                    PhysicalStorageCode = table.Column<string>(type: "TEXT", nullable: true),
                    DigitalStorageCode = table.Column<string>(type: "TEXT", nullable: true),
                    DigitalFileFormat = table.Column<string>(type: "TEXT", nullable: true),
                    DigitalResolution = table.Column<string>(type: "TEXT", nullable: true),
                    PosterUrl = table.Column<string>(type: "TEXT", nullable: true),
                    BackgroundUrl = table.Column<string>(type: "TEXT", nullable: true),
                    PersonalRating = table.Column<int>(type: "INTEGER", nullable: true),
                    TmdbId = table.Column<int>(type: "INTEGER", nullable: true),
                    TmdbRating = table.Column<double>(type: "REAL", nullable: true),
                    TmdbRatingCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    Runtime = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GenreVideo",
                columns: table => new
                {
                    GenresId = table.Column<int>(type: "INTEGER", nullable: false),
                    VideosId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreVideo", x => new { x.GenresId, x.VideosId });
                    table.ForeignKey(
                        name: "FK_GenreVideo_Genres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenreVideo_Videos_VideosId",
                        column: x => x.VideosId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Characters = table.Column<string>(type: "TEXT", nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    VideoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Roles_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Overview = table.Column<string>(type: "TEXT", nullable: true),
                    PosterUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TmdbId = table.Column<int>(type: "INTEGER", nullable: true),
                    TVShowId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Videos_TVShowId",
                        column: x => x.TVShowId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EpisodeNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Overview = table.Column<string>(type: "TEXT", nullable: true),
                    Runtime = table.Column<int>(type: "INTEGER", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DigitalFileFormat = table.Column<string>(type: "TEXT", nullable: true),
                    DigitalResolution = table.Column<string>(type: "TEXT", nullable: true),
                    PersonalRating = table.Column<int>(type: "INTEGER", nullable: true),
                    TmdbId = table.Column<int>(type: "INTEGER", nullable: true),
                    TmdbRating = table.Column<double>(type: "REAL", nullable: true),
                    TmdbRatingCount = table.Column<int>(type: "INTEGER", nullable: true),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episodes_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_SeasonId",
                table: "Episodes",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_GenreVideo_VideosId",
                table: "GenreVideo",
                column: "VideosId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_PersonId",
                table: "Roles",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_VideoId",
                table: "Roles",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_TVShowId",
                table: "Seasons",
                column: "TVShowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "GenreVideo");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Videos");
        }
    }
}
