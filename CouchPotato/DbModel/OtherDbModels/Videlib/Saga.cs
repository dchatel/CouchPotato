using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CouchPotato.DbModel.OtherDbModels.Videlib;

[Table("Sagas")]
public partial class Saga
{
    public Saga()
    {
        Films = new HashSet<Film>();
    }

    public string? BackdropPath { get; set; }
    public virtual ICollection<Film> Films { get; set; }
    public string? Name { get; set; }
    public int? SagaId { get; set; }
    public int? TmdbId { get; set; }
}