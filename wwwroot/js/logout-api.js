document.addEventListener('DOMContentLoaded', function() {
    const logoutBtn = document.getElementById('logoutBtn');
    if (!logoutBtn) return;
    logoutBtn.addEventListener('click', async function(e) {
        e.preventDefault();
        await fetch('/api/accountapi/logout', {
            method: 'POST',
            credentials: 'include'
        });
        // Показываем popup (если есть) или делаем редирект
        const popup = document.getElementById('logoutPopup');
        if (popup) {
            popup.style.display = 'flex';
        } else {
            window.location.href = '/';
        }
    });
}); 