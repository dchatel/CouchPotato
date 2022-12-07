using System.Collections.Generic;

namespace CouchPotato.DbModel.OtherDbModels.Videlib;

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