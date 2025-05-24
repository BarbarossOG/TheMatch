document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('form');
    if (!form) return;
    const genderMap = {
        "Мужчина": "М",
        "Женщина": "Ж"
    };
    form.addEventListener('submit', async function(e) {
        e.preventDefault();
        const housingMap = {
            own: "Есть своё",
            rent: "Арендую",
            family: "Живу с родителями",
            none: "Отсутствует"
        };
        const incomeMap = {
            none: "Отсутствует",
            low: "Низкий",
            "low-medium": "Ниже среднего",
            medium: "Средний",
            "high-medium": "Выше среднего",
            high: "Высокий"
        };
        const data = {
            имя: form.querySelector('[placeholder="Введите ваше полное имя"]').value,
            пол: genderMap[form.querySelector('input[name="gender"]:checked')?.nextElementSibling.innerText] || '',
            рост: parseInt(form.querySelector('[name="height"]').value),
            датаРождения: form.querySelector('[type="date"]').value,
            местоположение: form.querySelector('[placeholder="Введите ваш город"]').value,
            описание: '', // если появится поле, подставьте сюда
            уровеньЗаработка: incomeMap[form.querySelector('[name="income"]').value],
            жильё: housingMap[form.querySelector('[name="housing"]').value],
            наличиеДетей: form.querySelector('input[name="childs"]:checked')?.value === 'yes',
            электроннаяПочта: form.querySelector('[type="email"]').value,
            пароль: form.querySelector('[placeholder="Введите пароль"]').value
        };
        const response = await fetch('/api/accountapi/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const result = await response.text();
        alert(result);
    });
}); 