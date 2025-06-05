document.addEventListener('DOMContentLoaded', function() {
    const dialogsList = document.getElementById('chatDialogsList');
    const messagesBlock = document.getElementById('chatMessagesBlock');
    const sendForm = document.getElementById('chatSendForm');
    const messageInput = document.getElementById('chatMessageInput');
    const actionsBlock = document.getElementById('chatActions');
    const btnDate = document.getElementById('btnDate');
    const btnBlock = document.getElementById('btnBlock');
    let currentDialogId = null;
    let pollInterval = null;
    let myId = null;

    // --- Получение userId из query string ---
    function getQueryParam(name) {
        const url = new URL(window.location.href);
        return url.searchParams.get(name);
    }
    const initialUserId = getQueryParam('userId') ? +getQueryParam('userId') : null;

    // --- Получить данные пользователя по id (если нет в списке диалогов) ---
    function fetchUserById(userId) {
        return fetch(`/api/chatapi/userinfo/${userId}`)
            .then(r => r.json());
    }

    // Получаем id текущего пользователя и только после этого инициализируем чат
    fetch('/api/chatapi/myid')
        .then(r => r.json())
        .then(data => {
            myId = data.id;
            loadDialogs();
            setInterval(loadDialogs, 3000); // 3 секунды
        });

    // Загрузка диалогов
    function loadDialogs() {
        fetch('/api/chatapi/dialogs')
            .then(r => r.json())
            .then(async arr => {
                // Если есть initialUserId и его нет в arr, подгружаем пользователя
                if (initialUserId && !arr.some(u => u.idПользователя === initialUserId)) {
                    try {
                        const u = await fetchUserById(initialUserId);
                        if (u && u.имя) { // Проверка на валидность
                            arr.push({
                                idПользователя: u.id,
                                имя: u.имя,
                                фото: u.фото
                            });
                        }
                    } catch {}
                }
                if (!arr.length) {
                    dialogsList.innerHTML = '<div style="color:#b39ddb;padding:32px 0;text-align:center;">У вас пока нет переписок</div>';
                    return;
                }
                dialogsList.innerHTML = arr.map(u => `
                    <div class="chat-dialog-item${u.idПользователя===currentDialogId?' active':''}" data-id="${u.idПользователя}">
                        <img src="${u.фото||'/images/avatars/standart.jpg'}" class="avatar" alt="">
                        <span class="name">${u.имя}</span>
                        <span class="dialog-actions${u.idПользователя===currentDialogId?'':' d-none'}">
                            <button class="btn-dialog btn-dialog-date" title="Встречаться" data-action="date">❤</button>
                            <button class="btn-dialog btn-dialog-block" title="Заблокировать" data-action="block">🚫</button>
                        </span>
                    </div>
                `).join('');
                // Если initialUserId есть, сразу открыть этот диалог
                if (initialUserId && currentDialogId !== initialUserId) {
                    const item = dialogsList.querySelector(`.chat-dialog-item[data-id='${initialUserId}']`);
                    if (item) {
                        item.click();
                    } else {
                        // Если диалог только что добавлен, всё равно показываем форму
                        currentDialogId = initialUserId;
                        sendForm.classList.remove('d-none');
                        loadMessages(currentDialogId);
                        if (pollInterval) clearInterval(pollInterval);
                        pollInterval = setInterval(() => loadMessages(currentDialogId, false), 4000);
                    }
                }
            });
    }

    // Загрузка сообщений
    function loadMessages(dialogId, scrollToBottom = true) {
        fetch(`/api/chatapi/messages/${dialogId}`)
            .then(r => r.json())
            .then(arr => {
                console.log('myId:', myId, arr.map(m => [m.idСообщения, m.idОтправителя, m.прочитано]));
                if (!arr.length) {
                    messagesBlock.innerHTML = '<div class="chat-placeholder">Сообщений пока нет. Напишите первым!</div>';
                } else {
                    messagesBlock.innerHTML = arr.map(m => `
                        <div class="chat-message ${m.idОтправителя===myId?'sent':'received'}${m.прочитано ? ' read' : ''}">
                            <div class="text">${escapeHtml(m.текст)}</div>
                            <div class="date">${formatDate(m.датаОтправки)}${m.idОтправителя===myId && m.прочитано ? ' <span style=\"color:#888;font-size:0.95em;vertical-align:middle;\">&#10003;&#10003;</span>' : ''}</div>
                        </div>
                    `).join('');
                }
                if (scrollToBottom) messagesBlock.scrollTop = messagesBlock.scrollHeight;
            });
    }

    // Выбор диалога
    dialogsList.addEventListener('click', function(e) {
        const item = e.target.closest('.chat-dialog-item');
        if (!item) return;
        const id = +item.dataset.id;
        if (id === currentDialogId) return;
        currentDialogId = id;
        document.querySelectorAll('.chat-dialog-item').forEach(el => el.classList.remove('active'));
        item.classList.add('active');
        sendForm.classList.remove('d-none');
        loadMessages(currentDialogId);
        if (pollInterval) clearInterval(pollInterval);
        pollInterval = setInterval(() => loadMessages(currentDialogId, false), 4000);
        // Пометить как прочитанные
        fetch(`/api/chatapi/read/${currentDialogId}`, {method:'POST'}).then(()=>{
            loadMessages(currentDialogId, false);
        });
        // Показать кнопки только для активного диалога
        document.querySelectorAll('.dialog-actions').forEach(el => el.classList.add('d-none'));
        const actions = item.querySelector('.dialog-actions');
        if (actions) actions.classList.remove('d-none');
    });

    // Делегированные обработчики для кнопок в диалоге
    dialogsList.addEventListener('click', function(e) {
        const btn = e.target.closest('.btn-dialog');
        if (!btn) return;
        const item = btn.closest('.chat-dialog-item');
        if (!item) return;
        const dialogId = +item.dataset.id;
        if (btn.dataset.action === 'date') {
            btn.disabled = true;
            fetch('/api/chatapi/date', {
                method: 'POST',
                headers: {'Content-Type':'application/json'},
                body: JSON.stringify(dialogId)
            })
            .then(r => r.json())
            .then(data => {
                if (data.mutual) {
                    alert('У вас взаимное желание встречаться! Теперь вы не можете просматривать анкеты и писать другим пользователям.');
                } else {
                    alert('Вы предложили встречаться. Ждём ответа собеседника!');
                }
            })
            .finally(()=>{ btn.disabled = false; });
        }
        if (btn.dataset.action === 'block') {
            if (!confirm('Вы уверены, что хотите заблокировать пользователя и удалить переписку?')) return;
            btn.disabled = true;
            fetch('/api/chatapi/block', {
                method: 'POST',
                headers: {'Content-Type':'application/json'},
                body: JSON.stringify(dialogId)
            })
            .then(r => {
                if (r.ok) {
                    alert('Пользователь заблокирован и переписка удалена.');
                    // Удалить диалог из списка
                    item.remove();
                    // Если это был активный диалог — сбросить чат
                    if (dialogId === currentDialogId) {
                        sendForm.classList.add('d-none');
                        messagesBlock.innerHTML = '<div class="chat-placeholder">Пользователь заблокирован</div>';
                        currentDialogId = null;
                    }
                }
            })
            .finally(()=>{ btn.disabled = false; });
        }
    });

    // Отправка сообщения
    sendForm.addEventListener('submit', function(e) {
        e.preventDefault();
        const text = messageInput.value.trim();
        if (!text || !currentDialogId) return;
        fetch('/api/chatapi/send', {
            method: 'POST',
            headers: {'Content-Type':'application/json'},
            body: JSON.stringify({IdПолучателя: currentDialogId, Текст: text})
        })
        .then(r => {
            if (r.ok) {
                messageInput.value = '';
                setTimeout(() => loadMessages(currentDialogId), 300); // небольшая задержка
                loadDialogs(); // обновить список диалогов
            }
        });
    });

    // Вспомогательные функции
    function escapeHtml(str) {
        return str.replace(/[&<>"']/g, function(tag) {
            const chars = {'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;','\'':'&#39;'};
            return chars[tag]||tag;
        });
    }
    function formatDate(dateStr) {
        const d = new Date(dateStr);
        return d.toLocaleString('ru-RU', {day:'2-digit', month:'2-digit', hour:'2-digit', minute:'2-digit'});
    }
}); 