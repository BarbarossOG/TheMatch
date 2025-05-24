using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("Типы_операций")]
public partial class ТипыОпераций
{
    [Key]
    [Column("ID_Тип_операции")]
    public byte IdТипОперации { get; set; }

    [Column("Название_типа")]
    [StringLength(50)]
    public string НазваниеТипа { get; set; } = null!;

    [InverseProperty("IdТипОперацииNavigation")]
    public virtual ICollection<ЖурналДействий> ЖурналДействийs { get; set; } = new List<ЖурналДействий>();
}
