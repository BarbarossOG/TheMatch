document.addEventListener('DOMContentLoaded', function() {
    let allInterests = [];
    let allCities = [];
    let selectedInterests = [];
    let selectedCity = '';
    let members = [];
    let currentIndex = 0;

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
            console.log('Ответ сервера (анкеты):', arr);
            members = arr;
            currentIndex = 0;
            showCurrentMember();
        });
    }

    // --- Показ текущей анкеты ---
    function showCurrentMember() {
        const container = document.getElementById('memberCardContainer');
        const noMore = document.getElementById('noMoreMembers');
        console.log('Показываем анкету, members:', members, 'currentIndex:', currentIndex);
        if (!members.length) {
            container.innerHTML = `
            <div class="member-card">
                <div class="member-photo-block">
                    <img src="/images/avatars/standart.jpg" alt="member" class="member-photo">
                    <div class="member-name-age">Имя не указано</div>
                    <div class="member-actions">
                        <button class="member-action-btn skip" title="Пропустить"><span style="font-weight:bold;font-size:1.7rem;">&#10006;</span></button>
                        <button class="member-action-btn like" title="Лайк"><span style="font-weight:bold;font-size:1.7rem;">&#10084;</span></button>
                        <button class="member-action-btn dislike" title="Дизлайк"><span style="font-weight:bold;font-size:1.7rem;">&#128078;</span></button>
                    </div>
                </div>
                <div class="member-interests-block">
                    <div class="member-interests-title">Интересы</div>
                    <div class="member-interests-list">
                        <span style='color:#888;'>Нет интересов</span>
                    </div>
                </div>
            </div>`;
            noMore.style.display = 'block';
            return;
        }
        if (currentIndex >= members.length) {
            container.innerHTML = '';
            noMore.style.display = 'block';
            return;
        }
        noMore.style.display = 'none';
        const m = members[currentIndex];
        // Определяем массив фото: если есть m.фотографии (массив), используем его, иначе одно фото
        const photos = Array.isArray(m.фотографии) && m.фотографии.length ? m.фотографии : [m.фото];
        let photoIndex = 0;
        function renderPhotoBlock() {
            return `
                <div class="image-preview" style="position:relative; display:flex; align-items:center; justify-content:center;">
                    <button type="button" class="photo-prev-btn" style="position:absolute; left:-32px; top:50%; transform:translateY(-50%); display:${photos.length > 1 ? 'block' : 'none'};">&#8592;</button>
                    <img src="${photos[photoIndex]}" alt="member" class="member-photo">
                    <button type="button" class="photo-next-btn" style="position:absolute; right:-32px; top:50%; transform:translateY(-50%); display:${photos.length > 1 ? 'block' : 'none'};">&#8594;</button>
                </div>
            `;
        }
        container.innerHTML = `
        <div class="member-card">
            <div class="member-photo-block" id="memberPhotoBlock">
                ${renderPhotoBlock()}
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
        // Навигация по фото
        const photoBlock = document.getElementById('memberPhotoBlock');
        if (photoBlock) {
            photoBlock.addEventListener('click', function(e) {
                if (e.target.classList.contains('photo-prev-btn')) {
                    photoIndex = (photoIndex - 1 + photos.length) % photos.length;
                    photoBlock.innerHTML = renderPhotoBlock() + photoBlock.innerHTML.split('</div>')[1];
                } else if (e.target.classList.contains('photo-next-btn')) {
                    photoIndex = (photoIndex + 1) % photos.length;
                    photoBlock.innerHTML = renderPhotoBlock() + photoBlock.innerHTML.split('</div>')[1];
                }
            });
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