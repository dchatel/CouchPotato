using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

#nullable disable

namespace CouchPotato.DbModel.OtherDbModels.Videlib;

public partial class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cast> Casts { get; set; }
    public virtual DbSet<Episode> Episodes { get; set; }
    public virtual DbSet<Film> Films { get; set; }
    public virtual DbSet<Genre> Genres { get; set; }
    public virtual DbSet<Person> Persons { get; set; }
    public virtual DbSet<Saga> Sagas { get; set; }
    public virtual DbSet<Season> Seasons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;ApplicationIntent=ReadOnly;AttachDbFilename=C:\Users\dchat\Videlib.mdf");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NoChange).IsRequired();

            entity.HasMany(e => e.Films).WithMany(e => e.Genres)
                .UsingEntity<Dictionary<string, object>>(
                    e => e.HasOne<Film>().WithMany().HasForeignKey("FilmId"),
                    e => e.HasOne<Genre>().WithMany().HasForeignKey("GenreId"),
                    e =>
                    {
                        e.HasKey("FilmId", "GenreId");
                        e.ToTable("FilmGenre");
                    }
                );

            entity.HasData(
                new Genre { GenreId = 1, Name = "Divers", NoChange = true },
                new Genre { GenreId = 2, Name = "Action", NoChange = true },
                new Genre { GenreId = 3, Name = "Aventure", NoChange = true },
                new Genre { GenreId = 4, Name = "Animation", NoChange = true },
                new Genre { GenreId = 5, Name = "Comédie", NoChange = true },
                new Genre { GenreId = 6, Name = "Crime", NoChange = true },
                new Genre { GenreId = 7, Name = "Documentaire", NoChange = true },
                new Genre { GenreId = 8, Name = "Drame", NoChange = true },
                new Genre { GenreId = 9, Name = "Familial", NoChange = true },
                new Genre { GenreId = 10, Name = "Enfants", NoChange = true },
                new Genre { GenreId = 11, Name = "Fantastique", NoChange = true },
                new Genre { GenreId = 12, Name = "Guerre", NoChange = true },
                new Genre { GenreId = 13, Name = "Politique", NoChange = true },
                new Genre { GenreId = 14, Name = "Histoire", NoChange = true },
                new Genre { GenreId = 15, Name = "Horreur", NoChange = true },
                new Genre { GenreId = 16, Name = "Musique", NoChange = true },
                new Genre { GenreId = 17, Name = "Mystère", NoChange = true },
                new Genre { GenreId = 18, Name = "News", NoChange = true },
                new Genre { GenreId = 19, Name = "Réalité", NoChange = true },
                new Genre { GenreId = 20, Name = "Romance", NoChange = true },
                new Genre { GenreId = 21, Name = "Science-Fiction", NoChange = true },
                new Genre { GenreId = 22, Name = "Soap", NoChange = true },
                new Genre { GenreId = 23, Name = "Talk", NoChange = true },
                new Genre { GenreId = 24, Name = "Téléfilm", NoChange = true },
                new Genre { GenreId = 25, Name = "Thriller", NoChange = true },
                new Genre { GenreId = 26, Name = "Western", NoChange = true },
                new Genre { GenreId = 27, Name = "Erotique", NoChange = false },
                new Genre { GenreId = 28, Name = "Péplum", NoChange = false },
                new Genre { GenreId = 29, Name = "Suspense", NoChange = false },
                new Genre { GenreId = 30, Name = "Théâtre", NoChange = false },
                new Genre { GenreId = 31, Name = "Arts Martiaux", NoChange = false },
                new Genre { GenreId = 32, Name = "Catastrophe", NoChange = false },
                new Genre { GenreId = 33, Name = "Biographie", NoChange = false },
                new Genre { GenreId = 34, Name = "Médical", NoChange = false }
            );
        });

        modelBuilder.Entity<Saga>(entity =>
        {
            entity.Property(e => e.BackdropPath).HasMaxLength(200);  // Online
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Film>(entity =>
        {
            entity.Property(e => e.EType).HasMaxLength(10);
            entity.Property(e => e.Version).HasMaxLength(10);

            entity.Property(e => e.Place).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(50);

            entity.Property(e => e.Disk).HasMaxLength(10);
            entity.Property(e => e.Format).HasMaxLength(10);
            entity.Property(e => e.Resolution).HasMaxLength(20);

            // Tmdb
            entity.Property(e => e.BackdropPath).HasMaxLength(50);  // local Guid.ext
            entity.Property(e => e.Origin).HasMaxLength(2).IsFixedLength();
            entity.Property(e => e.OriginalLanguage).HasMaxLength(2).IsFixedLength();
            entity.Property(e => e.OriginalTitle).HasMaxLength(200);
            entity.Property(e => e.Overview);
            entity.Property(e => e.PosterPath).HasMaxLength(50);    // local Guid.ext
            entity.Property(e => e.ReleaseDate).HasColumnType("date");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Tagline).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(200);
            // TV
            entity.Property(e => e.LastAirDate).HasColumnType("date");

            // Movie
            entity.HasOne(d => d.Saga)
                .WithMany(p => p.Films)
                .HasForeignKey(d => d.SagaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.ProfilePath).HasMaxLength(200); // Online
        });

        modelBuilder.Entity<Cast>(entity =>
        {
            entity.HasKey(e => new { e.FilmId, e.PersonId });

            entity.Property(e => e.Characters).HasMaxLength(500);

            entity.HasOne(d => d.Film)
                .WithMany(p => p.Casts)
                .HasForeignKey(d => d.FilmId);

            entity.HasOne(d => d.Person)
                .WithMany(p => p.Casts)
                .HasForeignKey(d => d.PersonId);
        });

        modelBuilder.Entity<Season>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Overview);
            entity.Property(e => e.PosterPath).HasMaxLength(200);   // Online

            entity.HasOne(d => d.Film)
                .WithMany(p => p.Seasons)
                .HasForeignKey(d => d.FilmId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.Property(e => e.AirDate).HasColumnType("date");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Overview);
            entity.Property(e => e.StillPath).HasMaxLength(200);     // Online

            entity.HasOne(d => d.Season)
                .WithMany(p => p.Episodes)
                .HasForeignKey(d => d.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}