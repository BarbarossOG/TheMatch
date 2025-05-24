using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("Тип_взаимодействия")]
public partial class ТипВзаимодействия
{
    [Key]
    [Column("ID_Тип_взаимодействия")]
    public byte IdТипВзаимодействия { get; set; }

    [Column("Название_типа")]
    [StringLength(50)]
    public string НазваниеТипа { get; set; } = null!;

    [InverseProperty("IdТипВзаимодействияNavigation")]
    public virtual ICollection<ЖурналПриложения> ЖурналПриложенияs { get; set; } = new List<ЖурналПриложения>();
}
