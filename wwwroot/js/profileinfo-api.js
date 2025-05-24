let allHobbies = [];
let userInterests = [];

function renderInterestsModal(selected) {
    const modal = document.querySelector('.filter-checkboxes');
    if (!modal) return;
    modal.innerHTML = '';
    allHobbies.forEach(interest => {
        const checked = selected.includes(interest) ? 'checked' : '';
        modal.innerHTML += `
            <label><input type="checkbox" value="${interest}" ${checked}> ${interest}</label>
        `;
    });
}

function updateSelectedInterests(selected) {
    const selectedDiv = document.getElementById('selectedInterests');
    if (selected.length === 0) {
        selectedDiv.innerHTML = 'Интересы не выбраны';
    } else {
        selectedDiv.innerHTML = 'Вы выбрали: ' + selected.join(', ');
    }
}

function loadProfile() {
    // Сначала получаем все интересы из БД
    fetch('/api/accountapi/allhobbies')
        .then(r => r.json())
        .then(hobbies => {
            allHobbies = hobbies;
            // Затем получаем профиль пользователя
            return fetch('/api/accountapi/profileinfo');
        })
        .then(r => r.json())
        .then(data => {
            console.log('profileinfo data:', data);
            console.log('name:', document.getElementById('name'));
            console.log('height:', document.getElementById('height'));
            document.getElementById('name').value = data.имя || '';
            document.getElementById('height').value = data.рост || '';
            document.getElementById('birthdate').value = data.датаРождения ? data.датаРождения.substr(0,10) : '';
            document.getElementById('location').value = data.местоположение || '';
            document.getElementById('description').value = data.описание || '';
            document.getElementById('income').value = data.уровеньЗаработка || '';
            document.getElementById('housing').value = data.жильё || '';
            document.getElementById('children').value = data.наличиеДетей || '';
            userInterests = (data.интересы || []);
            renderInterestsModal(userInterests);
            updateSelectedInterests(userInterests);
        });
}

function saveProfile() {
    // Собираем интересы из модального окна
    const selectedInterests = Array.from(document.querySelectorAll('.filter-checkboxes input[type=checkbox]:checked')).map(cb => cb.value);
    fetch('/api/accountapi/profileinfo', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({
            имя: document.getElementById('name').value,
            рост: parseInt(document.getElementById('height').value) || null,
            датаРождения: document.getElementById('birthdate').value,
            местоположение: document.getElementById('location').value,
            описание: document.getElementById('description').value,
            уровеньЗаработка: document.getElementById('income').value,
            жильё: document.getElementById('housing').value,
            наличиеДетей: document.getElementById('children').value,
            интересы: selectedInterests
        })
    }).then(r => {
        if (r.ok) alert('Данные сохранены!');
        else alert('Ошибка сохранения');
    });
}

document.addEventListener('DOMContentLoaded', () => {
    loadProfile();
    document.getElementById('save-btn').addEventListener('click', e => {
        e.preventDefault();
        saveProfile();
    });
    // При открытии модального окна интересов — обновляем чекбоксы
    $(document).on('show.bs.modal', '#interestsModal', function () {
        renderInterestsModal(userInterests);
    });
    // При сохранении интересов из модального окна
    document.getElementById('saveInterestsBtn').addEventListener('click', function() {
        const selected = Array.from(document.querySelectorAll('.filter-checkboxes input[type=checkbox]:checked')).map(cb => cb.value);
        userInterests = selected;
        updateSelectedInterests(selected);
    });
}); 