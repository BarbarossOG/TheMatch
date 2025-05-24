using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("Подписка")]
public partial class Подписка
{
    [Key]
    [Column("ID_Подписки")]
    public byte IdПодписки { get; set; }

    [Column("Название_подписки")]
    [StringLength(50)]
    public string НазваниеПодписки { get; set; } = null!;

    public short Цена { get; set; }

    [Column("Срок_действия")]
    public byte СрокДействия { get; set; }

    [InverseProperty("IdПодпискиNavigation")]
    public virtual ICollection<КупленныеПодписки> КупленныеПодпискиs { get; set; } = new List<КупленныеПодписки>();
}
