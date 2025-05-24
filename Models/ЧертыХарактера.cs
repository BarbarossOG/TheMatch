using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Table("ЧертыХарактера")]
public partial class ЧертыХарактера
{
    [Key]
    [Column("ID_ЧертыХарактера")]
    public byte IdЧертыХарактера { get; set; }

    [Column("Название_черты")]
    [StringLength(30)]
    public string НазваниеЧерты { get; set; } = null!;

    [InverseProperty("IdЧертыХарактераNavigation")]
    public virtual ICollection<ЧертыПользователя> ЧертыПользователя { get; set; } = new List<ЧертыПользователя>();
}
