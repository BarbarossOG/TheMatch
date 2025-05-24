using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("Должность")]
public partial class Должность
{
    [Key]
    [Column("ID_Должности")]
    public byte IdДолжности { get; set; }

    [Column("Название_должности")]
    [StringLength(50)]
    public string НазваниеДолжности { get; set; } = null!;

    [InverseProperty("IdДолжностиNavigation")]
    public virtual ICollection<Модератор> Модераторs { get; set; } = new List<Модератор>();
}
