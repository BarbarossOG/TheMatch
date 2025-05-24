let allHobbies = [];
let userInterests = [];
let isEditMode = false;

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
}); 