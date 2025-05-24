using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[PrimaryKey("IdПользователя", "IdСообщения")]
[Table("Сообщения_пользователей")]
public partial class СообщенияПользователей
{
    [Key]
    [Column("ID_Пользователя")]
    public int IdПользователя { get; set; }

    [Key]
    [Column("ID_Сообщения")]
    public int IdСообщения { get; set; }

    [Column("ID_Типа_сообщения")]
    public byte IdТипаСообщения { get; set; }

    [ForeignKey("IdПользователя")]
    [InverseProperty("СообщенияПользователейs")]
    public virtual Пользователи IdПользователяNavigation { get; set; } = null!;

    [ForeignKey("IdСообщения")]
    [InverseProperty("СообщенияПользователейs")]
    public virtual Сообщения IdСообщенияNavigation { get; set; } = null!;

    [ForeignKey("IdТипаСообщения")]
    [InverseProperty("СообщенияПользователейs")]
    public virtual ТипСообщения IdТипаСообщенияNavigation { get; set; } = null!;
}
