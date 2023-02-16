using System.Collections.Generic;
using System.Windows;

using Microsoft.EntityFrameworkCore;

namespace CouchPotato.DbModel;

public class DataContext : DbContext
{
    public DbSet<Episode> Episodes { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Season> Seasons { get; set; } = null!;
    public DbSet<Video> Videos { get; set; } = null!;
    public DbSet<Movie> Movies { get; set; } = null!;
    public DbSet<TVShow> TVShows { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("NOCASE");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=couchpotato.db");
    }
}
