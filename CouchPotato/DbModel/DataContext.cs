using System.Collections.Generic;
using System.Windows;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace CouchPotato.DbModel;

public partial class DataContext : DbContext
{
    public DbSet<Episode> Episodes { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Season> Seasons { get; set; } = null!;
    public DbSet<Video> Videos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("NOCASE");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove(typeof(TableNameFromDbSetConvention));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(@"Data Source=couchpotato.db");
    }
}
