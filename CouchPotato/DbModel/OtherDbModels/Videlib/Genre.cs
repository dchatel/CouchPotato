using System.Collections.Generic;

namespace CouchPotato.DbModel.OtherDbModels.Videlib;

public partial class Genre
{
    public Genre()
    {
        Films = new HashSet<Film>();
    }

    public virtual ICollection<Film> Films { get; set; }
    public int? GenreId { get; set; }
    public string? Name { get; set; }
    public bool? NoChange { get; set; }
}