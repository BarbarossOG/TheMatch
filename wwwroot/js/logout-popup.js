document.addEventListener('DOMContentLoaded', function() {
    var logoutBtn = document.getElementById('logoutBtn');
    var logoutPopup = document.getElementById('logoutPopup');
    if (logoutBtn && logoutPopup) {
        logoutBtn.onclick = function() {
            logoutPopup.style.display = 'flex';
        };
    }
}); 