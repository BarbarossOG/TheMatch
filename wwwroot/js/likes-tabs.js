document.addEventListener('DOMContentLoaded', function() {
    // --- Проверка на mutual date ---
    fetch('/api/accountapi/searchmembers', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({})
    })
    .then(r => r.json())
    .then(resp => {
        if (resp && typeof resp === 'object' && resp.canView === false && resp.reason && resp.reason.includes('взаимное свидание')) {
            showMutualDateBlockLikes(resp.reason);
            return;
        }
        // Если нет блокировки — обычная логика
        initLikesTabs();
    });

    // --- Все переменные и функции объявляем здесь ---
    const tabWhoLikedMe = document.getElementById('tabWhoLikedMe');
    const tabMutualLikes = document.getElementById('tabMutualLikes');
    const whoLikedMeContainer = document.getElementById('whoLikedMeContainer');
    const mutualLikesContainer = document.getElementById('mutualLikesContainer');

    function setActiveTab(tab) {
        if (tab === 'who') {
            tabWhoLikedMe.classList.add('active');
            tabMutualLikes.classList.remove('active');
            whoLikedMeContainer.style.display = 'block';
            mutualLikesContainer.style.display = 'none';
        } else {
            tabWhoLikedMe.classList.remove('active');
            tabMutualLikes.classList.add('active');
            whoLikedMeContainer.style.display = 'none';
            mutualLikesContainer.style.display = 'block';
        }
    }

    function loadWhoLikedMe() {
        whoLikedMeContainer.innerHTML = '<div style="margin:30px auto;font-size:1.2em;color:#8e44ad;">Загрузка...</div>';
        fetch('/api/accountapi/likedtome')
            .then(r => r.json())
            .then(arr => {
                if (!arr.length) {
                    whoLikedMeContainer.innerHTML = '<div style="margin:30px auto;font-size:1.2em;color:#8e44ad;">Пока никто не лайкнул.</div>';
                    return;
                }
                whoLikedMeContainer.innerHTML = `<div class=\"mutual-likes-row\">` + arr.map(u => `
                    <div class=\"col-1-5\">
                        <div class=\"single-friend mutual-compact\" data-user-id=\"${u.id}\">
                            <img src=\"${u.фото}\" alt=\"\" class=\"profile-photo\">
                            <div class=\"content\">
                                <span class=\"name\">${u.имя}</span>
                                <div class=\"member-actions\" style=\"margin-left:auto;gap:10px;\">
                                    <button class=\"member-action-btn like\" title=\"Лайк\"><span style=\"font-weight:bold;font-size:1.7rem;\">&#10084;</span></button>
                                    <button class=\"member-action-btn dislike\" title=\"Дизлайк\"><span style=\"font-weight:bold;font-size:1.7rem;\">&#128078;</span></button>
                                </div>
                            </div>
                        </div>
                    </div>
                `).join('') + `</div>`;
                // Навешиваем обработчики
                whoLikedMeContainer.querySelectorAll('.member-action-btn.like').forEach(btn => {
                    btn.onclick = function(e) {
                        const card = btn.closest('.single-friend');
                        const userId = card.getAttribute('data-user-id');
                        sendInteraction(userId, 2, card); // Лайк
                    };
                });
                whoLikedMeContainer.querySelectorAll('.member-action-btn.dislike').forEach(btn => {
                    btn.onclick = function(e) {
                        const card = btn.closest('.single-friend');
                        const userId = card.getAttribute('data-user-id');
                        sendInteraction(userId, 3, card); // Дизлайк
                    };
                });
            });
    }

    function loadMutualLikes() {
        mutualLikesContainer.innerHTML = '<div style="margin:30px auto;font-size:1.2em;color:#8e44ad;">Загрузка...</div>';
        fetch('/api/accountapi/mutuallikes')
            .then(r => r.json())
            .then(arr => {
                if (!arr.length) {
                    mutualLikesContainer.innerHTML = '<div style="margin:30px auto;font-size:1.2em;color:#8e44ad;">Взаимных симпатий пока нет.</div>';
                    return;
                }
                mutualLikesContainer.innerHTML = `<div class=\"mutual-likes-row\">` + arr.map(u => `
                    <div class=\"col-1-5\">
                        <div class=\"single-friend mutual-compact\">
                            <img src=\"${u.фото}\" alt=\"\" class=\"profile-photo\">
                            <div class=\"content\">
                                <span class=\"name\">${u.имя}</span>
                                <a href=\"/Community/Chats?userId=${u.id}\" class=\"connnect-btn\">Связаться</a>
                            </div>
                        </div>
                    </div>
                `).join('') + `</div>`;
            });
    }

    function sendInteraction(targetUserId, interactionTypeId, cardElem) {
        fetch('/api/accountapi/userinteraction', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ TargetUserId: targetUserId, InteractionTypeId: interactionTypeId })
        })
        .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t); });
            // Удаляем карточку
            cardElem.parentElement.parentElement.remove();
        })
        .catch(e => {
            alert('Ошибка взаимодействия: ' + e.message);
        });
    }

    function showMutualDateBlockLikes(reason) {
        const main = document.querySelector('.community-section.inner-page .container');
        if (main) {
            main.innerHTML = `
            <div class=\"mutual-date-block\" style=\"max-width:520px;margin:60px auto 0 auto;padding:40px 32px 32px 32px;background:#faf8fd;border-radius:18px;box-shadow:0 2px 24px rgba(142,68,173,0.10);text-align:center;\">
                <div style=\"font-size:1.35em;color:#7c3aed;font-weight:600;margin-bottom:18px;\">${reason}</div>
                <div style=\"color:#6c3483;font-size:1.08em;margin-bottom:28px;\">Вы решили начать встречаться с другим пользователем. Просмотр симпатий заблокирован, чтобы вы могли сосредоточиться друг на друге.</div>
                <button id=\"btnForceViewLikes\" class=\"btn-force-view-likes\" style=\"background:#7c3aed;color:#fff;font-weight:600;font-size:1.12em;padding:12px 32px;border:none;border-radius:10px;box-shadow:0 2px 8px rgba(124,58,237,0.08);transition:background 0.15s;\">Хочу всё равно смотреть</button>
            </div>
            <div id=\"mutualDateModalLikes\" class=\"mutual-date-modal\" style=\"display:none;position:fixed;z-index:9999;left:0;top:0;width:100vw;height:100vh;background:rgba(60,16,100,0.18);align-items:center;justify-content:center;\">
                <div style=\"background:#fff;padding:32px 28px 24px 28px;border-radius:16px;max-width:400px;margin:auto;box-shadow:0 2px 24px rgba(124,58,237,0.13);text-align:center;\">
                    <div style=\"font-size:1.18em;color:#8e44ad;font-weight:600;margin-bottom:18px;\">Вы уверены?</div>
                    <div style=\"color:#6c3483;font-size:1.05em;margin-bottom:22px;\">Если вы продолжите, ваше решение встречаться с этим пользователем будет отменено, и вы снова сможете просматривать симпатии других пользователей.</div>
                    <button id=\"btnModalYesLikes\" class=\"btn btn-primary\" style=\"background:#7c3aed;color:#fff;font-weight:600;padding:10px 28px;border:none;border-radius:8px;margin-right:12px;\">Да, продолжить</button>
                    <button id=\"btnModalNoLikes\" class=\"btn btn-outline-secondary\" style=\"background:#f3e9ff;color:#7c3aed;font-weight:600;padding:10px 28px;border:none;border-radius:8px;\">Отмена</button>
                </div>
            </div>
            <style>
                .btn-force-view-likes:hover { background:#6c3483; }
            </style>
            `;
            document.getElementById('btnForceViewLikes').onclick = function() {
                document.getElementById('mutualDateModalLikes').style.display = 'flex';
            };
            document.getElementById('btnModalNoLikes').onclick = function() {
                document.getElementById('mutualDateModalLikes').style.display = 'none';
            };
            document.getElementById('btnModalYesLikes').onclick = function() {
                fetch('/api/accountapi/removedate', { method: 'POST' })
                    .then(r => r.ok ? location.reload() : alert('Ошибка. Попробуйте позже.'));
            };
        }
    }

    function initLikesTabs() {
        tabWhoLikedMe.addEventListener('click', function() {
            setActiveTab('who');
            loadWhoLikedMe();
        });
        tabMutualLikes.addEventListener('click', function() {
            setActiveTab('mutual');
            loadMutualLikes();
        });
        setActiveTab('who');
        loadWhoLikedMe();
    }
}); 