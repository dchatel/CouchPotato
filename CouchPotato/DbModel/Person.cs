using System.Collections.Generic;

namespace CouchPotato.DbModel;

public class Person
{
    public Person()
    {
        Roles = new HashSet<Role>();
    }

    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? PortraitUrl { get; set; }
    public int? TmdbId { get; set; }

    public ICollection<Role> Roles { get; set; }
}
