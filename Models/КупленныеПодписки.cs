using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[PrimaryKey("IdПользователя", "IdПодписки", "ДатаПокупки")]
[Table("Купленные_подписки")]
public partial class КупленныеПодписки
{
    [Key]
    [Column("ID_Пользователя")]
    public int IdПользователя { get; set; }

    [Key]
    [Column("ID_Подписки")]
    public byte IdПодписки { get; set; }

    [Key]
    [Column("Дата_покупки")]
    public DateOnly ДатаПокупки { get; set; }

    [Column("Дата_окончания")]
    public DateOnly ДатаОкончания { get; set; }

    [ForeignKey("IdПодписки")]
    [InverseProperty("КупленныеПодпискиs")]
    public virtual Подписка IdПодпискиNavigation { get; set; } = null!;

    [ForeignKey("IdПользователя")]
    [InverseProperty("КупленныеПодпискиs")]
    public virtual Пользователи IdПользователяNavigation { get; set; } = null!;
}
