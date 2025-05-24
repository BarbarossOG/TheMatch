using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Keyless]
public partial class УлучшеннаяМодерацияСообщений
{
    [Column("ID_Модератора")]
    public int IdМодератора { get; set; }

    [Column("ID_Сообщения")]
    public int IdСообщения { get; set; }

    [Column("ID_Пользователя_отправителя")]
    public int IdПользователяОтправителя { get; set; }

    [Column("Текст_сообщения")]
    [StringLength(3000)]
    public string ТекстСообщения { get; set; } = null!;

    [StringLength(30)]
    public string Решение { get; set; } = null!;

    [Column("Дата_модерации", TypeName = "smalldatetime")]
    public DateTime ДатаМодерации { get; set; }
}
