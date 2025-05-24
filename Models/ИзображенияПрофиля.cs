using System.ComponentModel.DataAnnotations.Schema;

namespace TheMatch.Models
{
    public class ИзображенияПрофиля
    {
        public int ID_Изображения { get; set; }
        public int ID_Пользователя { get; set; }
        public string Ссылка { get; set; }
        public bool Основное { get; set; }

        [ForeignKey(nameof(ID_Пользователя))]
        public virtual Пользователи Пользователь { get; set; }
    }
} 