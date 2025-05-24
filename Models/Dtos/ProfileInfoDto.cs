namespace TheMatch.Models.Dtos
{
    public class ProfileInfoDto
    {
        public string Имя { get; set; }
        public int? Рост { get; set; }
        public DateTime? ДатаРождения { get; set; }
        public string Местоположение { get; set; }
        public string Описание { get; set; }
        public string УровеньЗаработка { get; set; }
        public string Жильё { get; set; }
        public string НаличиеДетей { get; set; }
        public List<string> Интересы { get; set; }
    }
} 