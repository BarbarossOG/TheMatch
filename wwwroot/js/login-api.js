document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('form');
    if (!form) return;
    form.addEventListener('submit', async function(e) {
        e.preventDefault();
        const data = {
            электроннаяПочта: form.querySelector('[type="email"]').value,
            пароль: form.querySelector('[placeholder="Введите ваш пароль"]').value
        };
        const response = await fetch('/api/accountapi/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const result = await response.text();
        if (response.ok) {
            alert('Вход выполнен успешно!');
            window.location.href = '/Community/Members';
        } else {
            alert('Ошибка входа: ' + result);
        }
    });
}); 