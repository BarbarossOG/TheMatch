using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheMatch.Models
{
    [Table("Переписка")]
    public class Переписка
    {
        [Key]
        public int IdСообщения { get; set; }

        [Required]
        public int IdОтправителя { get; set; }

        [Required]
        public int IdПолучателя { get; set; }

        [Required]
        [MaxLength(250)]
        public string Текст { get; set; }

        [Required]
        public DateTime ДатаОтправки { get; set; }

        [Required]
        public bool Прочитано { get; set; }

        // Навигационные свойства (опционально)
        [ForeignKey("IdОтправителя")]
        public Пользователи Отправитель { get; set; }

        [ForeignKey("IdПолучателя")]
        public Пользователи Получатель { get; set; }
    }
} 