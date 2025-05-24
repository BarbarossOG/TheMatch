using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("Модератор")]
[Index("ЭлектроннаяПочта", Name = "UNIQUE_email_moders", IsUnique = true)]
public partial class Модератор
{
    [Key]
    [Column("ID_Модератора")]
    public int IdМодератора { get; set; }

    [Column("ID_Должности")]
    public byte IdДолжности { get; set; }

    [StringLength(50)]
    public string Имя { get; set; } = null!;

    [Column("Электронная_почта")]
    [StringLength(255)]
    public string ЭлектроннаяПочта { get; set; } = null!;

    [StringLength(30)]
    public string Пароль { get; set; } = null!;

    [ForeignKey("IdДолжности")]
    [InverseProperty("Модераторs")]
    public virtual Должность IdДолжностиNavigation { get; set; } = null!;

    [InverseProperty("IdМодератораNavigation")]
    public virtual ICollection<ЖурналДействий> ЖурналДействийs { get; set; } = new List<ЖурналДействий>();

    [InverseProperty("IdМодератораNavigation")]
    public virtual ICollection<МодерацияСообщений> МодерацияСообщенийs { get; set; } = new List<МодерацияСообщений>();
}
