using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Keyless]
public partial class ПользователиНазванияУвлечений
{
    [Column("ID_Пользователя")]
    public int IdПользователя { get; set; }

    [Column("Название_увлечения")]
    [StringLength(50)]
    public string НазваниеУвлечения { get; set; } = null!;
}
