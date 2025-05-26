document.addEventListener('DOMContentLoaded', function() {
    fetch('/api/accountapi/mutuallikes')
        .then(r => r.json())
        .then(arr => {
            const container = document.getElementById('mutualLikesContainer');
            if (!arr.length) {
                container.innerHTML = '<div style="margin:30px auto;font-size:1.2em;color:#8e44ad;">Взаимных симпатий пока нет.</div>';
                return;
            }
            container.innerHTML = `<div class=\"mutual-likes-row\">` + arr.map(u => `
                <div class=\"col-1-5\">
                    <div class=\"single-friend mutual-compact\">
                        <img src=\"${u.фото}\" alt=\"\" class=\"profile-photo\">
                        <div class=\"content\">
                            <span class=\"name\">${u.имя}</span>
                            <a href=\"#\" class=\"connnect-btn\">Связаться</a>
                        </div>
                    </div>
                </div>
            `).join('') + `</div>`;
        });
}); 