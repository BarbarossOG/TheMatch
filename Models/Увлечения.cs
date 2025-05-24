using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("Увлечения")]
public partial class Увлечения
{
    [Key]
    [Column("ID_Увлечения")]
    public byte IdУвлечения { get; set; }

    [Column("Название_увлечения")]
    [StringLength(50)]
    public string НазваниеУвлечения { get; set; } = null!;

    [ForeignKey("IdУвлечения")]
    [InverseProperty("IdУвлеченияs")]
    public virtual ICollection<Пользователи> IdПользователяs { get; set; } = new List<Пользователи>();
}
