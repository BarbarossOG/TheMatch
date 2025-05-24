using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[PrimaryKey("IdМодератора", "IdСообщения")]
[Table("Модерация_сообщений")]
public partial class МодерацияСообщений
{
    [Key]
    [Column("ID_Модератора")]
    public int IdМодератора { get; set; }

    [Key]
    [Column("ID_Сообщения")]
    public int IdСообщения { get; set; }

    [Column("Дата_и_время", TypeName = "smalldatetime")]
    public DateTime ДатаИВремя { get; set; }

    [StringLength(30)]
    public string Решение { get; set; } = null!;

    [ForeignKey("IdМодератора")]
    [InverseProperty("МодерацияСообщенийs")]
    public virtual Модератор IdМодератораNavigation { get; set; } = null!;

    [ForeignKey("IdСообщения")]
    [InverseProperty("МодерацияСообщенийs")]
    public virtual Сообщения IdСообщенияNavigation { get; set; } = null!;
}
