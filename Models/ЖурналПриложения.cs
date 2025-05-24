using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[PrimaryKey("IdПользователя1", "IdТипВзаимодействия", "IdПользователя2", "ДатаИВремя")]
[Table("Журнал_приложения")]
public partial class ЖурналПриложения
{
    [Key]
    [Column("ID_Пользователя1")]
    public int IdПользователя1 { get; set; }

    [Key]
    [Column("ID_Тип_взаимодействия")]
    public byte IdТипВзаимодействия { get; set; }

    [Key]
    [Column("ID_Пользователя2")]
    public int IdПользователя2 { get; set; }

    [Key]
    [Column("Дата_и_время", TypeName = "smalldatetime")]
    public DateTime ДатаИВремя { get; set; }

    [ForeignKey("IdПользователя1")]
    [InverseProperty("ЖурналПриложенияIdПользователя1Navigations")]
    public virtual Пользователи IdПользователя1Navigation { get; set; } = null!;

    [ForeignKey("IdПользователя2")]
    [InverseProperty("ЖурналПриложенияIdПользователя2Navigations")]
    public virtual Пользователи IdПользователя2Navigation { get; set; } = null!;

    [ForeignKey("IdТипВзаимодействия")]
    [InverseProperty("ЖурналПриложенияs")]
    public virtual ТипВзаимодействия IdТипВзаимодействияNavigation { get; set; } = null!;
}
