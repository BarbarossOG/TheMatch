document.getElementById('changePasswordForm').addEventListener('submit', function(e) {
    e.preventDefault();
    const oldPassword = document.getElementById('oldPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    fetch('/api/accountapi/changepassword', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ oldPassword, newPassword, confirmPassword })
    })
    .then(r => {
        if (!r.ok) return r.text().then(t => { throw new Error(t); });
        return r.text();
    })
    .then(() => {
        document.getElementById('changePasswordMsg').innerHTML = '<span style="color:green;">Пароль успешно изменён!</span>';
        document.getElementById('changePasswordForm').reset();
    })
    .catch(e => {
        document.getElementById('changePasswordMsg').innerHTML = '<span style="color:red;">' + e.message + '</span>';
    });
}); 