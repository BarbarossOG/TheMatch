document.addEventListener('DOMContentLoaded', function() {
    // --- обработка формы входа ---
    const form = document.querySelector('form');
    if (form) {
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
    }

    // --- обработка формы сброса пароля ---
    const resetForm = document.getElementById('reset-password-form');
    if (resetForm) {
        resetForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            const email = document.getElementById('reset-email').value;
            const password = document.getElementById('reset-password').value;
            const secret = document.getElementById('reset-secret').value;
            const msg = document.getElementById('reset-password-message');
            msg.textContent = '';
            console.log('[RESET] submit', { email, password, secret });
            try {
                const response = await fetch('/api/accountapi/resetpassword', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email, password, secret })
                });
                const result = await response.text();
                console.log('[RESET] response', response.status, result);
                if (response.ok) {
                    msg.style.color = 'green';
                    msg.textContent = 'Пароль успешно изменён!';
                    setTimeout(()=>$('#resetPasswordModal').modal('hide'), 1500);
                } else {
                    msg.style.color = 'red';
                    msg.textContent = result;
                }
            } catch (err) {
                console.error('[RESET] fetch error', err);
                msg.style.color = 'red';
                msg.textContent = 'Ошибка отправки запроса';
            }
        });
    }
}); 