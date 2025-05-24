using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[PrimaryKey("IdПользователя", "IdЧертыХарактера")]
[Table("Черты_пользователя")]
public partial class ЧертыПользователя
{
    [Key]
    [Column("ID_Пользователя")]
    public int IdПользователя { get; set; }

    [Key]
    [Column("ID_ЧертыХарактера")]
    public byte IdЧертыХарактера { get; set; }

    [Column(TypeName = "decimal(5, 3)")]
    public decimal Значение { get; set; }

    [ForeignKey("IdПользователя")]
    [InverseProperty("ЧертыПользователя")]
    public virtual Пользователи IdПользователяNavigation { get; set; } = null!;

    [ForeignKey("IdЧертыХарактера")]
    [InverseProperty("ЧертыПользователя")]
    public virtual ЧертыХарактера IdЧертыХарактераNavigation { get; set; } = null!;
}
