public class RegisterDto
{
    public string Имя { get; set; }
    public string Пол { get; set; }
    public byte Рост { get; set; }
    public DateOnly ДатаРождения { get; set; }
    public string Местоположение { get; set; }
    public string? Описание { get; set; }
    public string УровеньЗаработка { get; set; }
    public string Жильё { get; set; }
    public bool НаличиеДетей { get; set; }
    public string ЭлектроннаяПочта { get; set; }
    public string Пароль { get; set; }
} 