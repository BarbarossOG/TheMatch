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
        // Добавьте любые другие поля, которые могут быть обязательными
    };
    // The ... field is required.
    msg = msg.replace(/The ([^ ]+) field is required\./g, function(_, field) {
        if (field.toLowerCase() === 'dto') return '';
        return `Поле "${fieldMap[field] || field}" обязательно для заполнения.`;
    });
    // dto системное
    msg = msg.replace(/dto/i, '');
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
    // Попробовать распарсить строку как JSON, если это строка
    if (typeof errorData === 'string') {
        try {
            errorData = JSON.parse(errorData);
        } catch {
            // не JSON, оставляем строку
        }
    }
    // Для отладки
    console.log('parseAndShowErrors errorData:', errorData);

    if (errorData && errorData.errors) {
        for (const [key, arr] of Object.entries(errorData.errors)) {
            for (let m of arr) {
                // dto — системное, не показываем
                if (key === 'dto' || m.toLowerCase().includes('dto')) continue;
                // Если ключ вида $.Рост или $.ДатаРождения — показываем понятное сообщение
                const match = key.match(/\$\.([А-Яа-яA-Za-z]+)/);
                if (match) {
                    const field = match[1];
                    if (field === 'Рост') {
                        messages.push('Поле "Рост" заполнено некорректно или не заполнено.');
                        continue;
                    }
                    if (field === 'ДатаРождения') {
                        messages.push('Поле "Дата рождения" заполнено некорректно или не заполнено.');
                        continue;
                    }
                    // Можно добавить другие поля по аналогии
                }
                // Убираем системные сообщения
                if (/DateOnly|Path:|LineNumber:|BytePositionInLine:|\$\./i.test(m)) continue;
                m = translateFieldError(m);
                if (!messages.includes(m) && m.trim() && m !== 'Проверьте правильность заполнения всех полей.') messages.push(m);
            }
        }
        // Если после фильтрации не осталось сообщений, но есть ошибки по полям — показываем их значения
        if (!messages.length) {
            for (const arr of Object.values(errorData.errors)) {
                for (let m of arr) {
                    if (!/DateOnly|Path:|LineNumber:|BytePositionInLine:|\$\./i.test(m) && m.trim()) {
                        messages.push(m);
                    }
                }
            }
        }
    } else if (errorData && errorData.title) {
        messages.push(translateFieldError(errorData.title));
    } else if (typeof errorData === 'string') {
        // --- Улучшенная регулярка для неуникального email ---
        if (/почт(а|ой)?.*существу/i.test(errorData) || /email.*существу/i.test(errorData) || /email.*зарегистрирован/i.test(errorData) || /email.*уже/i.test(errorData)) {
            messages.push('Этот email уже зарегистрирован. Пожалуйста, используйте другой.');
        } else {
            let msg = translateFieldError(errorData);
            if (!/DateOnly|Path:|LineNumber:|BytePositionInLine:|\$\./i.test(msg) && msg.trim() && msg !== 'Проверьте правильность заполнения всех полей.') {
                messages.push(msg);
            }
        }
    }
    // Убираем дубли и пустые строки
    messages = messages.filter((m, i, arr) => m && arr.indexOf(m) === i);
    if (!messages.length) {
        messages.push('Проверьте правильность заполнения всех полей!');
    }
    showToast(messages.map(m => `<div style='margin-bottom:4px;'>${m}</div>`).join(''), '#e74c3c');
}

async function checkEmailUnique(email) {
    try {
        const resp = await fetch(`/api/accountapi/checkemail?email=${encodeURIComponent(email)}`);
        if (!resp.ok) return false;
        const data = await resp.json();
        return !data.exists;
    } catch {
        return false;
    }
}

function validateFormFields(form) {
    const errors = [];
    if (!form.querySelector('[placeholder="Введите ваше полное имя"]').value.trim()) errors.push('Имя обязательно для заполнения');
    if (!form.querySelector('[type="date"]').value.trim()) errors.push('Дата рождения обязательна');
    if (!form.querySelector('[name="height"]').value.trim()) errors.push('Рост обязателен для заполнения');
    if (!form.querySelector('[placeholder="Введите ваш город"]').value.trim()) errors.push('Город обязателен для заполнения');
    if (!form.querySelector('[name="income"]').value.trim()) errors.push('Уровень заработка обязателен');
    if (!form.querySelector('[name="housing"]').value.trim()) errors.push('Жильё обязательно');
    if (!form.querySelector('[type="email"]').value.trim()) errors.push('Электронная почта обязательна');
    if (!form.querySelector('[placeholder="Введите пароль"]').value.trim()) errors.push('Пароль обязателен');
    if (form.querySelector('[placeholder="Введите пароль"]').value.length > 0 && form.querySelector('[placeholder="Введите пароль"]').value.length < 6) errors.push('Пароль должен быть не менее 6 символов');
    if (!form.querySelector('[placeholder="Подтвердите пароль"]').value.trim()) errors.push('Подтвердите пароль');
    // Проверка email на валидность
    const email = form.querySelector('[type="email"]').value.trim();
    if (email && !/^\S+@\S+\.\S+$/.test(email)) errors.push('Введите корректный email');
    // Проверка выбранного пола
    if (!form.querySelector('input[name="gender"]:checked')) errors.push('Пол обязателен для заполнения');
    // Проверка выбранного наличия детей
    if (!form.querySelector('input[name="childs"]:checked')) errors.push('Необходимо указать наличие детей');
    return errors;
}

document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('form');
    if (!form) return;
    const genderMap = {
        "Мужчина": "М",
        "Женщина": "Ж"
    };
    // --- Email uniqueness check on blur ---
    const emailInput = form.querySelector('[type="email"]');
    if (emailInput) {
        emailInput.addEventListener('blur', async function() {
            const email = emailInput.value.trim();
            if (!email) return;
            const isUnique = await checkEmailUnique(email);
            if (!isUnique) {
                showToast('Этот email уже зарегистрирован. Пожалуйста, используйте другой.', '#e74c3c');
                emailInput.classList.add('is-invalid');
            } else {
                emailInput.classList.remove('is-invalid');
            }
        });
    }
    // --- Prevent submit if email is not unique ---
    form.addEventListener('submit', async function(e) {
        e.preventDefault();
        const errors = validateFormFields(form);
        if (errors.length) {
            showToast(errors.map(m => `<div style='margin-bottom:4px;'>${m}</div>`).join(''), '#e74c3c');
            return;
        }
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
                    let errorData;
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/json')) {
                        errorData = await response.json();
                    } else {
                        errorData = await response.text();
                    }
                    parseAndShowErrors(errorData);
                    console.error('Registration error:', errorData); // Для отладки
                } catch (err) {
                    showToast('Проверьте правильность заполнения всех полей.', '#e74c3c');
                    console.error('Registration error (catch):', err);
                }
                return; // Не сбрасываем форму, просто показываем ошибку
            } else {
                showToast('Регистрация прошла успешно! Сейчас вы будете перенаправлены на страницу входа.', '#27ae60');
                setTimeout(() => { window.location.href = '/Account/Login'; }, 2000);
            }
        } catch (err) {
            showToast('Ошибка соединения с сервером. Попробуйте позже.', '#e74c3c');
        }
    });
}); 