using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[PrimaryKey("IdМодератора", "IdПользователя", "IdТипОперации", "ДатаИВремя")]
[Table("Журнал_действий")]
public partial class ЖурналДействий
{
    [Key]
    [Column("ID_Модератора")]
    public int IdМодератора { get; set; }

    [Key]
    [Column("ID_Пользователя")]
    public int IdПользователя { get; set; }

    [Key]
    [Column("ID_Тип_операции")]
    public byte IdТипОперации { get; set; }

    [StringLength(255)]
    public string? Причина { get; set; }

    [Key]
    [Column("Дата_и_время", TypeName = "smalldatetime")]
    public DateTime ДатаИВремя { get; set; }

    [Column("Дата_и_время_окончания", TypeName = "smalldatetime")]
    public DateTime? ДатаИВремяОкончания { get; set; }

    [ForeignKey("IdМодератора")]
    [InverseProperty("ЖурналДействийs")]
    public virtual Модератор IdМодератораNavigation { get; set; } = null!;

    [ForeignKey("IdПользователя")]
    [InverseProperty("ЖурналДействийs")]
    public virtual Пользователи IdПользователяNavigation { get; set; } = null!;

    [ForeignKey("IdТипОперации")]
    [InverseProperty("ЖурналДействийs")]
    public virtual ТипыОпераций IdТипОперацииNavigation { get; set; } = null!;
}
