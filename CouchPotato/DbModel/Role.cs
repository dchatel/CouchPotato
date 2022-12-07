using TMDbLib.Objects.General;

namespace CouchPotato.DbModel;

public class Role
{
    public int Id { get; set; }
    public string? Characters { get; set; }
    public int Order { get; set; }
    public Person Person { get; set; } = null!;
    public int PersonId { get; set; }
    public Video Video { get; set; } = null!;
    public int VideoId { get; set; }
}
