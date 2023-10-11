using System.ComponentModel.DataAnnotations.Schema;

namespace CouchPotato.DbModel.OtherDbModels.Videlib;

[Table("Casts")]
public partial class Cast
{
    public string? Characters { get; set; }
    public virtual Film? Film { get; set; }
    public int? FilmId { get; set; }
    public int? Order { get; set; }
    public virtual Person? Person { get; set; }
    public int? PersonId { get; set; }
}