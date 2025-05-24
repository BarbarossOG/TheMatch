using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("Пользователи")]
[Index("ЭлектроннаяПочта", Name = "UNIQUE_email_users", IsUnique = true)]
public partial class Пользователи
{
    [Key]
    [Column("ID_Пользователя")]
    public int IdПользователя { get; set; }

    [StringLength(50)]
    public string Имя { get; set; } = null!;

    [StringLength(10)]
    public string Пол { get; set; } = null!;

    public byte Рост { get; set; }

    [Column("Дата_рождения")]
    public DateOnly ДатаРождения { get; set; }

    [StringLength(50)]
    public string Местоположение { get; set; } = null!;

    [StringLength(255)]
    public string? Описание { get; set; }

    [Column("Уровень_заработка")]
    [StringLength(30)]
    public string УровеньЗаработка { get; set; } = null!;

    [StringLength(30)]
    public string Жильё { get; set; } = null!;

    [Column("Наличие_детей")]
    public bool НаличиеДетей { get; set; }

    [Column("Электронная_почта")]
    [StringLength(255)]
    public string ЭлектроннаяПочта { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string Пароль { get; set; } = null!;

    [InverseProperty("IdПользователяNavigation")]
    public virtual ICollection<ЖурналДействий> ЖурналДействийs { get; set; } = new List<ЖурналДействий>();

    [InverseProperty("IdПользователя1Navigation")]
    public virtual ICollection<ЖурналПриложения> ЖурналПриложенияIdПользователя1Navigations { get; set; } = new List<ЖурналПриложения>();

    [InverseProperty("IdПользователя2Navigation")]
    public virtual ICollection<ЖурналПриложения> ЖурналПриложенияIdПользователя2Navigations { get; set; } = new List<ЖурналПриложения>();

    [InverseProperty("IdПользователяNavigation")]
    public virtual ICollection<КупленныеПодписки> КупленныеПодпискиs { get; set; } = new List<КупленныеПодписки>();

    [InverseProperty("IdПользователяNavigation")]
    public virtual ICollection<СообщенияПользователей> СообщенияПользователейs { get; set; } = new List<СообщенияПользователей>();

    [InverseProperty("IdПользователяNavigation")]
    public virtual ICollection<ЧертыПользователя> ЧертыПользователяs { get; set; } = new List<ЧертыПользователя>();

    [ForeignKey("IdПользователя")]
    [InverseProperty("IdПользователяs")]
    public virtual ICollection<Увлечения> IdУвлеченияs { get; set; } = new List<Увлечения>();
}
