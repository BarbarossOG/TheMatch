document.getElementById('closeAccountForm').addEventListener('submit', function(e) {
    e.preventDefault();
    const reason = document.getElementById('deleteReason').value;
    const description = document.getElementById('deleteDescription').value;
    const confirm = document.getElementById('confirmClosure').checked;
    if (!confirm) {
        document.getElementById('closeAccountMsg').innerHTML = '<span style="color:red;">Вы должны подтвердить удаление аккаунта!</span>';
        return;
    }
    fetch('/api/accountapi/deleteaccount', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ reason, description, confirm })
    })
    .then(r => {
        if (!r.ok) return r.text().then(t => { throw new Error(t); });
        return r.text();
    })
    .then(() => {
        document.getElementById('closeAccountMsg').innerHTML = '<span style="color:green;">Ваш аккаунт удалён. Перенаправление...</span>';
        setTimeout(() => { window.location.href = '/'; }, 2000);
    })
    .catch(e => {
        document.getElementById('closeAccountMsg').innerHTML = '<span style="color:red;">' + e.message + '</span>';
    });
}); 