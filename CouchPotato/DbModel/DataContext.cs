using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Windows;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove(typeof(TableNameFromDbSetConvention));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var conn = new SqliteConnection(@"Data Source=couchpotato.db");
        conn.Open();
        conn.CreateCollation("NO_ACCENTS", StringHelper.AccentInsensitiveComparison);
        conn.CreateCollation("RESOLUTION_LESSER", StringHelper.IsResolutionLesser);
        conn.CreateCollation("RESOLUTION_EQUAL", StringHelper.IsResolutionEqual);
        conn.CreateCollation("RESOLUTION_GREATER", StringHelper.IsResolutionGreater);
        optionsBuilder
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .UseSqlite(conn);
    }
}
