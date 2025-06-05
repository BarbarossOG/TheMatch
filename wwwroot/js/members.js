document.addEventListener('DOMContentLoaded', function() {
    let allInterests = [];
    let allCities = [];
    let selectedInterests = [];
    let selectedCity = '';
    let members = [];
    let currentIndex = 0;

    // --- Проверка допуска к просмотру анкет ---
    fetch('/api/accountapi/canviewmembers')
        .then(r => r.json())
        .then(res => {
            if (!res.canView) {
                document.querySelector('.member-main-container').innerHTML = `<div style="margin:60px auto;max-width:500px;padding:32px 24px;background:#fff;border-radius:16px;box-shadow:0 2px 16px rgba(142,68,173,0.08);font-size:1.25em;text-align:center;color:#8e44ad;">${res.reason}</div>`;
                return;
            }
            // --- Подгружаем интересы ---
            fetch('/api/accountapi/allhobbies')
                .then(r => r.json())
                .then(arr => {
                    allInterests = arr;
                    renderInterestsCheckboxes();
                });

            // --- Подгружаем города ---
            fetch('/api/accountapi/allcities')
                .then(r => r.json())
                .then(arr => {
                    allCities = arr;
                    renderCitiesCheckboxes();
                });
        });

    // --- Рендер чекбоксов интересов ---
    function renderInterestsCheckboxes() {
        const box = document.getElementById('interestsCheckboxes');
        box.innerHTML = '';
        allInterests.forEach(interest => {
            const id = 'interest_' + interest.replace(/\s/g, '_');
            box.innerHTML += `<label><input type="checkbox" value="${interest}" id="${id}"> ${interest}</label>`;
        });
        // Ограничение до 3
        box.querySelectorAll('input[type=checkbox]').forEach(cb => {
            cb.addEventListener('change', function() {
                let checked = box.querySelectorAll('input[type=checkbox]:checked');
                if (checked.length > 3) {
                    this.checked = false;
                }
            });
        });
    }

    // --- Рендер чекбоксов городов ---
    function renderCitiesCheckboxes() {
        const box = document.getElementById('citiesCheckboxes');
        box.innerHTML = '';
        allCities.forEach(city => {
            const id = 'city_' + city.replace(/\s/g, '_');
            box.innerHTML += `<label><input type="radio" name="cityRadio" value="${city}" id="${id}"> ${city}</label>`;
        });
        // Обновлять selectedCity сразу при выборе
        box.querySelectorAll('input[type=radio]').forEach(rb => {
            rb.addEventListener('change', function() {
                selectedCity = this.value;
                document.getElementById('selectedCity').innerText = selectedCity || 'Не выбран';
                console.log('Выбран город (radio):', selectedCity);
            });
        });
    }

    // --- Сохранение выбранных интересов ---
    document.getElementById('saveInterestsBtn').addEventListener('click', function() {
        const checked = Array.from(document.querySelectorAll('#interestsCheckboxes input[type=checkbox]:checked')).map(cb => cb.value);
        selectedInterests = checked;
        document.getElementById('selectedInterests').innerText = checked.length ? checked.join(', ') : 'Не выбрано';
    });

    // --- Сохранение выбранного города ---
    document.getElementById('saveCityBtn').addEventListener('click', function() {
        const checked = document.querySelector('#citiesCheckboxes input[type=radio]:checked');
        selectedCity = checked ? checked.value : '';
        document.getElementById('selectedCity').innerText = selectedCity || 'Не выбран';
        console.log('Выбран город:', selectedCity);
    });

    // --- Фильтр ---
    document.getElementById('memberFilterForm').addEventListener('submit', function(e) {
        e.preventDefault();
        loadMembers();
    });

    // --- Загрузка анкет ---
    function loadMembers() {
        const ageMin = parseInt(document.getElementById('ageMin').value);
        const ageMax = parseInt(document.getElementById('ageMax').value);
        const heightMin = parseInt(document.getElementById('heightMin').value);
        const heightMax = parseInt(document.getElementById('heightMax').value);
        const filter = {
            City: selectedCity,
            AgeMin: ageMin,
            AgeMax: ageMax,
            HeightMin: heightMin,
            HeightMax: heightMax,
            Interests: selectedInterests
        };
        console.log('Фильтр для поиска анкет:', filter);
        fetch('/api/accountapi/searchmembers', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(filter)
        })
        .then(r => r.json())
        .then(arr => {
            // Новый блок: если сервер вернул объект с canView === false
            const container = document.getElementById('memberCardContainer');
            const noMore = document.getElementById('noMoreMembers');
            if (arr && typeof arr === 'object' && arr.canView === false) {
                if (arr.reason && arr.reason.includes('взаимное свидание')) {
                    showMutualDateBlock(arr.reason);
                } else if (container) {
                    container.innerHTML = `<div style="margin:60px auto;max-width:500px;padding:32px 24px;background:#fff;border-radius:16px;box-shadow:0 2px 16px rgba(142,68,173,0.08);font-size:1.25em;text-align:center;color:#8e44ad;">${arr.reason || 'Просмотр анкет недоступен.'}</div>`;
                }
                members = [];
                currentIndex = 0;
                if (noMore) noMore.style.display = 'none';
                return;
            }
            // Обычная логика
            members = arr;
            currentIndex = 0;
            showCurrentMember();
        });
    }

    // --- Показ текущей анкеты ---
    function showCurrentMember() {
        const container = document.getElementById('memberCardContainer');
        const noMore = document.getElementById('noMoreMembers');
        if (!members.length) {
            if (container) container.innerHTML = '';
            if (noMore) noMore.style.display = 'block';
            return;
        }
        if (currentIndex >= members.length) {
            if (container) container.innerHTML = '';
            if (noMore) noMore.style.display = 'block';
            return;
        }
        if (noMore) noMore.style.display = 'none';
        const m = members[currentIndex];
        // Определяем массив фото: если есть m.фотографии (массив), используем его, иначе одно фото
        const photos = Array.isArray(m.фотографии) && m.фотографии.length ? m.фотографии : [m.фото];
        let photoIndex = 0;
        container.innerHTML = `
        <div class="member-card">
            <div class="member-photo-block" id="memberPhotoBlock">
                <div class="image-preview" style="position:relative; display:flex; align-items:center; justify-content:center;">
                    <button type="button" class="custom-button photo-prev-btn" style="position:absolute; left:-40px; top:50%; transform:translateY(-50%);${photos.length > 1 ? '' : 'display:none;'};width:17px;text-align:center;vertical-align:middle;">&#8592;</button>
                    <img src="${photos[photoIndex]}" alt="member" class="member-photo" id="memberPhotoImg" style="max-width:265px; max-height:350px; width:265px; height:350px; object-fit:cover; border-radius:8px; border:1px solid #ccc;">
                    <button type="button" class="custom-button photo-next-btn" style="position:absolute; right:-40px; top:50%; transform:translateY(-50%);${photos.length > 1 ? '' : 'display:none;'};width:17px;text-align:center;vertical-align:middle;">&#8594;</button>
                </div>
                <div class="compatibility-rect">
                    <span>${m.совместимость !== undefined ? Math.round(m.совместимость) + '%' : '—'}</span>
                    <div class="compatibility-label">Совместимость</div>
                </div>
                <div class="member-name-age">${m.имя}, ${m.возраст}</div>
                <div class="member-actions">
                    <button class="member-action-btn skip" title="Пропустить"><span style="font-weight:bold;font-size:1.7rem;">&#10006;</span></button>
                    <button class="member-action-btn like" title="Лайк"><span style="font-weight:bold;font-size:1.7rem;">&#10084;</span></button>
                    <button class="member-action-btn dislike" title="Дизлайк"><span style="font-weight:bold;font-size:1.7rem;">&#128078;</span></button>
                    <button class="member-action-btn block" title="Заблокировать" style="margin-left:10px;color:#c0392b;font-size:1.3em;">&#128683;</button>
                </div>
            </div>
            <div class="member-interests-block">
                <div class="member-interests-title">Интересы</div>
                <div class="member-interests-list">
                    ${m.интересы && m.интересы.length ? m.интересы.map(i => `<span class="member-interest">${i}</span>`).join('') : '<span style="color:#888;">Нет интересов</span>'}
                </div>
                <div style="margin-top:18px;font-size:1.08em;">
                    <div><b>Описание:</b></div>
                    <div class="member-description-box">${m.описание ? m.описание : '—'}</div>
                    <div><b>Рост:</b> ${m.рост || '—'} см</div>
                    <div><b>Уровень заработка:</b> ${m.уровеньЗаработка || '—'}</div>
                    <div><b>Жильё:</b> ${m.жильё || '—'}</div>
                    <div><b>Дети:</b> ${m.наличиеДетей || '—'}</div>
                    <div><b>Город:</b> ${m.город || '—'}</div>
                </div>
            </div>
        </div>`;
        // Навигация по анкетам
        container.querySelector('.member-action-btn.skip').onclick = nextMember;
        container.querySelector('.member-action-btn.like').onclick = function() {
            sendInteraction(m.id, 2); // Лайк
            nextMember();
        };
        container.querySelector('.member-action-btn.dislike').onclick = function() {
            sendInteraction(m.id, 3); // Дизлайк
            nextMember();
        };
        container.querySelector('.member-action-btn.block').onclick = function() {
            if (confirm('Заблокировать этого пользователя?')) {
                sendInteraction(m.id, 6); // Блокировка
                nextMember();
            }
        };
        // Навигация по фото (только меняем src у <img>)
        const photoImg = document.getElementById('memberPhotoImg');
        const prevBtn = document.querySelector('.photo-prev-btn');
        const nextBtn = document.querySelector('.photo-next-btn');
        if (prevBtn) {
            prevBtn.onclick = function() {
                photoIndex = (photoIndex - 1 + photos.length) % photos.length;
                photoImg.src = photos[photoIndex];
            };
        }
        if (nextBtn) {
            nextBtn.onclick = function() {
                photoIndex = (photoIndex + 1) % photos.length;
                photoImg.src = photos[photoIndex];
            };
        }
    }
    function nextMember() {
        currentIndex++;
        showCurrentMember();
    }

    // --- Автозагрузка при первом входе ---
    loadMembers();

    const heightMinInput = document.getElementById('heightMin');
    const heightMaxInput = document.getElementById('heightMax');
    heightMinInput.max = '230';
    heightMaxInput.max = '230';
    if (parseInt(heightMaxInput.value) < 230) heightMaxInput.value = '230';
});

function sendInteraction(targetUserId, interactionTypeId) {
    fetch('/api/accountapi/userinteraction', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ TargetUserId: targetUserId, InteractionTypeId: interactionTypeId })
    })
    .then(r => {
        if (!r.ok) return r.text().then(t => { throw new Error(t); });
        return r.text();
    })
    .catch(e => {
        alert('Ошибка взаимодействия: ' + e.message);
    });
}

function showMutualDateBlock(reason) {
    const container = document.getElementById('memberCardContainer') || document.body;
    container.innerHTML = `
    <div class="mutual-date-block" style="max-width:520px;margin:60px auto 0 auto;padding:40px 32px 32px 32px;background:#faf8fd;border-radius:18px;box-shadow:0 2px 24px rgba(142,68,173,0.10);text-align:center;">
        <div style="font-size:1.35em;color:#7c3aed;font-weight:600;margin-bottom:18px;">${reason}</div>
        <div style="color:#6c3483;font-size:1.08em;margin-bottom:28px;">Вы решили начать встречаться с другим пользователем. Просмотр анкет заблокирован, чтобы вы могли сосредоточиться друг на друге.</div>
        <button id="btnForceViewMembers" class="btn-force-view-members" style="background:#7c3aed;color:#fff;font-weight:600;font-size:1.12em;padding:12px 32px;border:none;border-radius:10px;box-shadow:0 2px 8px rgba(124,58,237,0.08);transition:background 0.15s;">Хочу всё равно смотреть</button>
    </div>
    <div id="mutualDateModal" class="mutual-date-modal" style="display:none;position:fixed;z-index:9999;left:0;top:0;width:100vw;height:100vh;background:rgba(60,16,100,0.18);align-items:center;justify-content:center;">
        <div style="background:#fff;padding:32px 28px 24px 28px;border-radius:16px;max-width:400px;margin:auto;box-shadow:0 2px 24px rgba(124,58,237,0.13);text-align:center;">
            <div style="font-size:1.18em;color:#8e44ad;font-weight:600;margin-bottom:18px;">Вы уверены?</div>
            <div style="color:#6c3483;font-size:1.05em;margin-bottom:22px;">Если вы продолжите, ваше решение встречаться с этим пользователем будет отменено, и вы снова сможете просматривать анкеты других пользователей.</div>
            <button id="btnModalYes" class="btn btn-primary" style="background:#7c3aed;color:#fff;font-weight:600;padding:10px 28px;border:none;border-radius:8px;margin-right:12px;">Да, продолжить</button>
            <button id="btnModalNo" class="btn btn-outline-secondary" style="background:#f3e9ff;color:#7c3aed;font-weight:600;padding:10px 28px;border:none;border-radius:8px;">Отмена</button>
        </div>
    </div>
    <style>
        .btn-force-view-members:hover { background:#6c3483; }
    </style>
    `;
    document.getElementById('btnForceViewMembers').onclick = function() {
        document.getElementById('mutualDateModal').style.display = 'flex';
    };
    document.getElementById('btnModalNo').onclick = function() {
        document.getElementById('mutualDateModal').style.display = 'none';
    };
    document.getElementById('btnModalYes').onclick = function() {
        fetch('/api/accountapi/removedate', { method: 'POST' })
            .then(r => r.ok ? location.reload() : alert('Ошибка. Попробуйте позже.'));
    };
} 