using System.ComponentModel.DataAnnotations.Schema;

namespace TheMatch.Models
{
    [Table("Увлечения_пользователя")]
    public class УвлеченияПользователя
    {
        [Column("ID_Пользователя")]
        public int ID_Пользователя { get; set; }
        [Column("ID_Увлечения")]
        public byte ID_Увлечения { get; set; }

        public Пользователи Пользователь { get; set; }
        public Увлечения Увлечение { get; set; }
    }
} 