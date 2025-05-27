document.addEventListener('DOMContentLoaded', function() {
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

    tabWhoLikedMe.addEventListener('click', function() {
        setActiveTab('who');
        loadWhoLikedMe();
    });
    tabMutualLikes.addEventListener('click', function() {
        setActiveTab('mutual');
        loadMutualLikes();
    });

    // --- Загрузка "Кто лайкнул меня" ---
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

    // --- Загрузка взаимных лайков (старая логика) ---
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

    // --- Отправка лайка/дизлайка ---
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

    // --- Автозагрузка первой вкладки ---
    setActiveTab('who');
    loadWhoLikedMe();
}); 