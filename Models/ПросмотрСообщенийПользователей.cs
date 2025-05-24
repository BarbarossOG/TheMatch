using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Keyless]
public partial class ПросмотрСообщенийПользователей
{
    [Column("ID_Сообщения")]
    public int IdСообщения { get; set; }

    [Column("Текст_сообщения")]
    [StringLength(3000)]
    public string ТекстСообщения { get; set; } = null!;

    [Column("Тип_сообщения")]
    [StringLength(50)]
    public string ТипСообщения { get; set; } = null!;

    [Column("Дата_и_время", TypeName = "smalldatetime")]
    public DateTime ДатаИВремя { get; set; }

    [Column("ID_Пользователя")]
    public int IdПользователя { get; set; }

    [Column("Имя_Пользователя")]
    [StringLength(50)]
    public string ИмяПользователя { get; set; } = null!;

    [Column("Пол_Пользователя")]
    [StringLength(10)]
    public string ПолПользователя { get; set; } = null!;

    [Column("Местоположение_Пользователя")]
    [StringLength(50)]
    public string МестоположениеПользователя { get; set; } = null!;
}
