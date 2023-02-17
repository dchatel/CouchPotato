using System.Collections.Generic;

namespace CouchPotato.DbModel;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool Fixed { get; set; } = false;

    public ICollection<Video> Videos { get; set; } = new HashSet<Video>();
}
