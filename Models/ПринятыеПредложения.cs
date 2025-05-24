using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheMatch.Models;

[Keyless]
public partial class ПринятыеПредложения
{
    [Column("Предложение_по_улучшению")]
    [StringLength(3000)]
    public string ПредложениеПоУлучшению { get; set; } = null!;
}
