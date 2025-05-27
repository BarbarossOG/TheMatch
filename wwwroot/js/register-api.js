function showToast(msg, color = '#e74c3c') {
    let toast = document.getElementById('toast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'toast';
        toast.style.position = 'fixed';
        toast.style.top = '30px';
        toast.style.right = '30px';
        toast.style.zIndex = '9999';
        toast.style.minWidth = '220px';
        toast.style.maxWidth = '400px';
        toast.style.padding = '16px 24px';
        toast.style.background = '#fff';
        toast.style.borderRadius = '10px';
        toast.style.boxShadow = '0 2px 16px rgba(0,0,0,0.08)';
        toast.style.fontWeight = '600';
        toast.style.fontSize = '1.08em';
        toast.style.wordBreak = 'break-word';
        document.body.appendChild(toast);
    }
    toast.style.display = 'block';
    toast.style.border = `1.5px solid ${color}`;
    toast.style.color = color;
    toast.innerHTML = msg;
    clearTimeout(toast._timeout);
    toast._timeout = setTimeout(() => { toast.style.display = 'none'; }, 5000);
}

function translateFieldError(msg) {
    const fieldMap = {
        'Жильё': 'Жильё',
        'УровеньЗаработка': 'Уровень заработка',
        'Имя': 'Имя',
        'Пол': 'Пол',
        'Рост': 'Рост',
        'ДатаРождения': 'Дата рождения',
        'Местоположение': 'Город',
        'ЭлектроннаяПочта': 'Электронная почта',
        'Пароль': 'Пароль',
        'ПодтверждениеПароля': 'Подтверждение пароля',
    };
    // The ... field is required.
    msg = msg.replace(/The ([^ ]+) field is required\./g, function(_, field) {
        return `Поле "${fieldMap[field] || field}" обязательно для заполнения.`;
    });
    // Email already taken
    msg = msg.replace(/already exists|already taken|already registered|already in use/gi, 'уже зарегистрирован');
    // Not a valid email
    msg = msg.replace(/not a valid email|invalid email/gi, 'Введите корректный email.');
    // Age/Date of birth < 18
    msg = msg.replace(/must be at least 18|should be at least 18|You must be at least 18|Возраст должен быть не менее 18/gi, 'Вам должно быть не менее 18 лет.');
    // Password too short
    msg = msg.replace(/must be at least (\d+) characters|должен содержать не менее (\d+) символов/gi, 'Пароль должен содержать не менее $1 символов.');
    // Required
    msg = msg.replace(/is required/gi, 'обязательно для заполнения');
    // JSON/DTO errors
    msg = msg.replace(/The JSON value could not be converted to [^\.]+\./g, 'Проверьте правильность заполнения всех полей.');
    msg = msg.replace(/Поле "dto" обязательно для заполнения\./g, 'Проверьте правильность заполнения всех полей.');
    return msg;
}

function parseAndShowErrors(errorData) {
    let messages = [];
    if (errorData.errors) {
        // ASP.NET style: errors = { field: [msg, ...], ... }
        for (const arr of Object.values(errorData.errors)) {
            for (let m of arr) {
                m = translateFieldError(m);
                if (!messages.includes(m)) messages.push(m);
            }
        }
    } else if (errorData.title) {
        messages.push(translateFieldError(errorData.title));
    } else if (typeof errorData === 'string') {
        messages.push(translateFieldError(errorData));
    }
    if (!messages.length) {
        messages.push('Проверьте правильность заполнения всех полей!');
    }
    showToast(messages.map(m => `<div style='margin-bottom:4px;'>${m}</div>`).join(''), '#e74c3c');
}

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
        // Получаем значения всех полей
        const имя = form.querySelector('[placeholder="Введите ваше полное имя"]').value;
        const пол = genderMap[form.querySelector('input[name="gender"]:checked')?.nextElementSibling.innerText] || '';
        const ростStr = form.querySelector('[name="height"]').value;
        const рост = ростStr ? Number(ростStr) : null;
        const датаРождения = form.querySelector('[type="date"]').value;
        const местоположение = form.querySelector('[placeholder="Введите ваш город"]').value;
        const уровеньЗаработка = incomeMap[form.querySelector('[name="income"]').value];
        const жильё = housingMap[form.querySelector('[name="housing"]').value];
        const наличиеДетей = form.querySelector('input[name="childs"]:checked')?.value === 'yes';
        const электроннаяПочта = form.querySelector('[type="email"]').value;
        const пароль = form.querySelector('[placeholder="Введите пароль"]').value;
        const подтверждениеПароля = form.querySelector('[placeholder="Подтвердите пароль"]').value;

        const data = {
            Имя: имя,
            Пол: пол,
            Рост: рост,
            ДатаРождения: датаРождения,
            Местоположение: местоположение,
            Описание: '',
            УровеньЗаработка: уровеньЗаработка,
            Жильё: жильё,
            НаличиеДетей: наличиеДетей,
            ЭлектроннаяПочта: электроннаяПочта,
            Пароль: пароль,
            ПодтверждениеПароля: подтверждениеПароля
        };

        try {
            const response = await fetch('/api/accountapi/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                try {
                    const errorData = await response.json();
                    parseAndShowErrors(errorData);
                } catch {
                    showToast('Проверьте правильность заполнения всех полей.', '#e74c3c');
                }
                return;
            }

            showToast('Регистрация прошла успешно! Сейчас вы будете перенаправлены на страницу входа.', '#27ae60');
            setTimeout(() => { window.location.href = '/Account/Login'; }, 2000);
        } catch (err) {
            showToast('Ошибка соединения с сервером. Попробуйте позже.', '#e74c3c');
        }
    });
}); 