let allHobbies = [];
let userInterests = [];
let isEditMode = false;

// === Слайдер фото профиля ===
let profilePhotos = [];
let currentPhotoIdx = 0;

function setProfileFieldsDisabled(disabled) {
    document.getElementById('name').disabled = disabled;
    document.getElementById('height').disabled = disabled;
    document.getElementById('birthdate').disabled = disabled;
    document.getElementById('location').disabled = disabled;
    document.getElementById('description').disabled = disabled;
    document.getElementById('income-select').disabled = disabled;
    document.getElementById('housing-select').disabled = disabled;
    Array.from(document.querySelectorAll('#children-radio input')).forEach(r => r.disabled = disabled);
    document.getElementById('choose-interests-btn').disabled = disabled;
    // Меняем цвет кнопки "Выбрать интересы"
    const btn = document.getElementById('choose-interests-btn');
    if (disabled) {
        btn.classList.remove('custom-button-edit');
        btn.classList.add('custom-button-disabled');
    } else {
        btn.classList.remove('custom-button-disabled');
        btn.classList.add('custom-button-edit');
    }
}

function renderInterestsModal(selected) {
    const modal = document.querySelector('.filter-checkboxes');
    if (!modal) return;
    modal.innerHTML = '<div class="interests-grid"></div>';
    const grid = modal.querySelector('.interests-grid');
    allHobbies.forEach(interest => {
        const checked = selected.includes(interest) ? 'checked' : '';
        grid.innerHTML += `
            <label><input type="checkbox" value="${interest}" ${checked}> ${interest}</label>
        `;
    });
    // Блокируем чекбоксы если не режим редактирования
    Array.from(modal.querySelectorAll('input[type=checkbox]')).forEach(cb => cb.disabled = !isEditMode);
}

function updateSelectedInterests(selected) {
    // Для нового красивого блока
    const listBox = document.getElementById('interests-list-box');
    if (listBox) {
        listBox.innerHTML = '';
        if (selected.length === 0) {
            listBox.innerHTML = '<span style="color:#888;">Интересы не выбраны</span>';
        } else {
            selected.forEach(interest => {
                const span = document.createElement('span');
                span.className = 'member-interest';
                span.textContent = interest;
                listBox.appendChild(span);
            });
        }
    }
    // Для старого блока (если остался)
    const selectedDiv = document.getElementById('selectedInterests');
    if (selectedDiv) {
        if (selected.length === 0) {
            selectedDiv.innerHTML = 'Интересы не выбраны';
        } else {
            selectedDiv.innerHTML = selected.join(', ');
        }
    }
}

function loadProfile() {
    fetch('/api/accountapi/allhobbies')
        .then(r => r.json())
        .then(hobbies => {
            allHobbies = hobbies;
            return fetch('/api/accountapi/profileinfo');
        })
        .then(r => r.json())
        .then(data => {
            document.getElementById('name').value = data.имя || '';
            document.getElementById('height').value = data.рост || '';
            document.getElementById('birthdate').value = data.датаРождения ? data.датаРождения.substr(0,10) : '';
            document.getElementById('location').value = data.местоположение || '';
            document.getElementById('description').value = data.описание || '';
            document.getElementById('income-select').value = data.уровеньЗаработка || '';
            document.getElementById('housing-select').value = data.жильё || '';
            if (data.наличиеДетей === 'Есть') {
                document.getElementById('children_yes').checked = true;
            } else {
                document.getElementById('children_no').checked = true;
            }
            userInterests = (data.интересы || []);
            renderInterestsModal(userInterests);
            updateSelectedInterests(userInterests);
            setProfileFieldsDisabled(true);
            document.getElementById('save-btn').style.display = 'none';
            document.getElementById('edit-btn').style.display = '';
            isEditMode = false;
        });
}

function saveProfile() {
    const selectedInterests = Array.from(document.querySelectorAll('.filter-checkboxes input[type=checkbox]:checked')).map(cb => cb.value);
    const income = document.getElementById('income-select').value;
    const housing = document.getElementById('housing-select').value;
    let children = document.getElementById('children_yes').checked ? 'Есть' : 'Нет';
    const profileData = {
        имя: document.getElementById('name').value,
        рост: parseInt(document.getElementById('height').value) || null,
        датаРождения: document.getElementById('birthdate').value,
        местоположение: document.getElementById('location').value,
        описание: document.getElementById('description').value,
        уровеньЗаработка: income,
        жильё: housing,
        наличиеДетей: children,
        интересы: selectedInterests
    };
    console.log('Отправляемые данные профиля:', profileData);
    fetch('/api/accountapi/profileinfo', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(profileData)
    }).then(r => {
        if (r.ok) {
            alert('Данные сохранены!');
            loadProfile();
        } else alert('Ошибка сохранения');
    });
}

// === Загрузка и смена фото профиля ===
function updateProfilePhotoPreview() {
    fetch('/api/accountapi/profilephoto')
        .then(r => r.json())
        .then(obj => {
            document.getElementById('imagePreview').src = obj.url;
        });
}

function loadProfilePhotos() {
    fetch('/api/accountapi/profilephotos')
        .then(r => r.json())
        .then(arr => {
            profilePhotos = arr;
            if (profilePhotos.length === 0) {
                document.getElementById('imagePreview').src = '/images/avatars/standart.jpg';
                document.getElementById('photo-prev').style.display = 'none';
                document.getElementById('photo-next').style.display = 'none';
            } else {
                if (currentPhotoIdx >= profilePhotos.length) currentPhotoIdx = profilePhotos.length - 1;
                showProfilePhoto(currentPhotoIdx);
                document.getElementById('photo-prev').style.display = profilePhotos.length > 1 ? '' : 'none';
                document.getElementById('photo-next').style.display = profilePhotos.length > 1 ? '' : 'none';
            }
        });
}

function showProfilePhoto(idx) {
    if (profilePhotos.length === 0) {
        document.getElementById('imagePreview').src = '/images/avatars/standart.jpg';
        return;
    }
    currentPhotoIdx = idx;
    document.getElementById('imagePreview').src = profilePhotos[idx].url + '?t=' + Date.now();
    document.getElementById('photo-prev').style.display = profilePhotos.length > 1 ? '' : 'none';
    document.getElementById('photo-next').style.display = profilePhotos.length > 1 ? '' : 'none';
}

// --- Новая асинхронная проверка лимита фото ---
async function canUploadMorePhotos() {
    const response = await fetch('/api/accountapi/profilephotos');
    const arr = await response.json();
    return arr.length < 2;
}

document.addEventListener('DOMContentLoaded', () => {
    loadProfile();
    document.getElementById('edit-btn').addEventListener('click', e => {
        setProfileFieldsDisabled(false);
        document.getElementById('save-btn').style.display = '';
        document.getElementById('edit-btn').style.display = 'none';
        isEditMode = true;
        renderInterestsModal(userInterests);
    });
    document.getElementById('save-btn').addEventListener('click', e => {
        e.preventDefault();
        setProfileFieldsDisabled(true);
        document.getElementById('save-btn').style.display = 'none';
        document.getElementById('edit-btn').style.display = '';
        isEditMode = false;
        saveProfile();
    });
    // При открытии модального окна интересов — обновляем чекбоксы
    $(document).on('show.bs.modal', '#interestsModal', function () {
        renderInterestsModal(userInterests);
    });
    // При сохранении интересов из модального окна
    document.getElementById('saveInterestsBtn').addEventListener('click', function() {
        if (!isEditMode) return;
        const selected = Array.from(document.querySelectorAll('.filter-checkboxes input[type=checkbox]:checked')).map(cb => cb.value);
        userInterests = selected;
        updateSelectedInterests(selected);
    });
    loadProfilePhotos();
    // Стрелки
    document.getElementById('photo-prev').addEventListener('click', function() {
        if (profilePhotos.length > 1) {
            currentPhotoIdx = (currentPhotoIdx - 1 + profilePhotos.length) % profilePhotos.length;
            showProfilePhoto(currentPhotoIdx);
        }
    });
    document.getElementById('photo-next').addEventListener('click', function() {
        if (profilePhotos.length > 1) {
            currentPhotoIdx = (currentPhotoIdx + 1) % profilePhotos.length;
            showProfilePhoto(currentPhotoIdx);
        }
    });
    // Кнопка удалить
    document.querySelector('.remove-button').addEventListener('click', function() {
        if (profilePhotos.length === 0) return;
        const photoId = profilePhotos[currentPhotoIdx].id;
        if (!confirm('Удалить это фото?')) return;
        fetch('/api/accountapi/deleteprofilephoto', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify(photoId)
        })
        .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t); });
            return r.text();
        })
        .then(() => {
            alert('Фото удалено!');
            loadProfilePhotos();
        })
        .catch(e => alert('Ошибка удаления: ' + e.message));
    });
    // Обработка выбора файла (добавляем ограничение на 2 фото)
    document.getElementById('profileImage').addEventListener('change', async function() {
        if (!await canUploadMorePhotos()) {
            alert('Можно загрузить только 2 фотографии!');
            this.value = '';
            return;
        }
        const file = this.files[0];
        if (!file) return;
        if (file.size > 2 * 1024 * 1024) {
            alert('Максимальный размер файла 2 МБ');
            return;
        }
        // Проверка размера изображения
        const img = new Image();
        img.onload = function() {
            if (img.width !== 265 || img.height !== 350) {
                alert('Фото должно быть размером ровно 265x350 пикселей!');
                return;
            }
            const formData = new FormData();
            formData.append('file', file);
            fetch('/api/accountapi/uploadprofilephoto', {
                method: 'POST',
                body: formData
            })
            .then(r => {
                if (!r.ok) return r.text().then(t => { throw new Error(t); });
                return r.json();
            })
            .then(obj => {
                alert('Фото успешно обновлено!');
                loadProfilePhotos();
            })
            .catch(e => alert('Ошибка загрузки: ' + e.message));
        };
        img.onerror = function() {
            alert('Ошибка чтения изображения!');
        };
        img.src = URL.createObjectURL(file);
    });
});

// --- Экспортируем функции для других страниц ---
window.getAllHobbies = function() {
    return fetch('/api/accountapi/allhobbies').then(r => r.json());
}
window.getAllCities = function() {
    return fetch('/api/accountapi/allcities').then(r => r.json());
} 