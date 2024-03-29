﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CouchPotato.DbModel.OtherDbModels.Videlib;

[Table("Persons")]
public partial class Person
{
    public Person()
    {
        Casts = new HashSet<Cast>();
    }

    public virtual ICollection<Cast> Casts { get; set; }
    public string? Name { get; set; }
    public int? PersonId { get; set; }
    public string? ProfilePath { get; set; }
    public int? TmdbId { get; set; }
}