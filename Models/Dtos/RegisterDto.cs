using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    public string Имя { get; set; }

    [Required(ErrorMessage = "Пол обязателен для заполнения")]
    public string Пол { get; set; }

    [Required(ErrorMessage = "Рост обязателен для заполнения")]
    [Range(150, 250, ErrorMessage = "Рост должен быть от 150 до 250 см")]
    public byte Рост { get; set; }

    [Required(ErrorMessage = "Дата рождения обязательна")]
    [DataType(DataType.Date)]
    [CustomValidation(typeof(RegisterDto), nameof(ValidateAge))]
    public DateOnly ДатаРождения { get; set; }

    [Required(ErrorMessage = "Город обязателен для заполнения")]
    public string Местоположение { get; set; }

    public string? Описание { get; set; }

    [Required(ErrorMessage = "Уровень заработка обязателен")]
    public string УровеньЗаработка { get; set; }

    [Required(ErrorMessage = "Жильё обязательно")]
    public string Жильё { get; set; }

    [Required(ErrorMessage = "Наличие детей обязательно")]
    public bool НаличиеДетей { get; set; }

    [Required(ErrorMessage = "Электронная почта обязательна")]
    [EmailAddress(ErrorMessage = "Введите корректный email")]
    public string ЭлектроннаяПочта { get; set; }

    [Required(ErrorMessage = "Пароль обязателен")]
    [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
    public string Пароль { get; set; }

    [Required(ErrorMessage = "Подтвердите пароль")]
    [Compare("Пароль", ErrorMessage = "Пароли не совпадают")]
    public string ПодтверждениеПароля { get; set; }

    public static ValidationResult? ValidateAge(DateOnly date, ValidationContext context)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - date.Year - (today.DayOfYear < date.DayOfYear ? 1 : 0);
        if (age < 18)
            return new ValidationResult("Вам должно быть не менее 18 лет");
        return ValidationResult.Success;
    }
} 