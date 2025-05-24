using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Keyless]
public partial class НеактивныеПользователи
{
    [Column("ID_Пользователя")]
    public int IdПользователя { get; set; }

    [StringLength(50)]
    public string Имя { get; set; } = null!;

    [Column("Последнее_взаимодействие", TypeName = "smalldatetime")]
    public DateTime? ПоследнееВзаимодействие { get; set; }
}
