using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("Тип_сообщения")]
public partial class ТипСообщения
{
    [Key]
    [Column("ID_Типа_сообщения")]
    public byte IdТипаСообщения { get; set; }

    [Column("Название_типа")]
    [StringLength(50)]
    public string НазваниеТипа { get; set; } = null!;

    [InverseProperty("IdТипаСообщенияNavigation")]
    public virtual ICollection<СообщенияПользователей> СообщенияПользователейs { get; set; } = new List<СообщенияПользователей>();
}
