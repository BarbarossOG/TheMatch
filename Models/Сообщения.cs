using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("Сообщения")]
public partial class Сообщения
{
    [Key]
    [Column("ID_Сообщения")]
    public int IdСообщения { get; set; }

    [Column("Текст_сообщения")]
    [StringLength(3000)]
    public string ТекстСообщения { get; set; } = null!;

    [Column("Дата_и_время", TypeName = "smalldatetime")]
    public DateTime ДатаИВремя { get; set; }

    [InverseProperty("IdСообщенияNavigation")]
    public virtual ICollection<МодерацияСообщений> МодерацияСообщенийs { get; set; } = new List<МодерацияСообщений>();

    [InverseProperty("IdСообщенияNavigation")]
    public virtual ICollection<СообщенияПользователей> СообщенияПользователейs { get; set; } = new List<СообщенияПользователей>();
}
