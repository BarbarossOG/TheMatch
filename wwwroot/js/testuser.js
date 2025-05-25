document.addEventListener('DOMContentLoaded', function() {
    let traitNameToId = {};
    let allTraitsList = [];
    fetch('/api/accountapi/alltraits')
        .then(r => r.json())
        .then(traits => {
            traits.forEach(t => { traitNameToId[t.name.trim().toLowerCase()] = t.id; });
            allTraitsList = traits;
        });

    const formContainer = document.getElementById('testUserFormContainer');
    const resultDiv = document.getElementById('userTraitsResult');
    const retakeBtn = document.getElementById('retakeTestBtn');
    const MAX_SCORE = 20;

    function showResults(userTraits) {
        // userTraits: [{id, name, value}]
        let traitMap = {};
        userTraits.forEach(t => { traitMap[t.id] = t.value; });
        let html = '<h4>Ваши черты:</h4><ul style="font-size:1.1em;">';
        allTraitsList.forEach(trait => {
            let val = traitMap[trait.id] || 0;
            html += `<li><b>${trait.name}</b>: ${val} / ${MAX_SCORE}</li>`;
        });
        html += '</ul>';
        resultDiv.innerHTML = html;
        resultDiv.style.display = 'block';
        retakeBtn.style.display = 'inline-block';
        formContainer.style.display = 'none';
    }

    function showTest() {
        resultDiv.style.display = 'none';
        retakeBtn.style.display = 'none';
        formContainer.style.display = 'block';
    }

    // Проверяем, есть ли результаты
    fetch('/api/accountapi/usertraits')
        .then(r => r.json())
        .then(traits => {
            if (traits && traits.length > 0) {
                showResults(traits);
            } else {
                showTest();
            }
        })
        .catch(() => { showTest(); });

    retakeBtn.addEventListener('click', function() {
        showTest();
    });

    document.getElementById('testUserForm').addEventListener('submit', function(e) {
        e.preventDefault();
        let allAnswered = true;
        let traitScores = {};
        for (let i = 1; i <= 12; i++) {
            const val = document.querySelector('input[name="q' + i + '"]:checked');
            if (!val) {
                allAnswered = false;
                break;
            }
            val.value.split(',').forEach(pair => {
                let [score, trait] = pair.split(':');
                trait = trait.trim().toLowerCase();
                score = parseFloat(score.replace(',', '.'));
                if (!traitScores[trait]) traitScores[trait] = 0;
                traitScores[trait] += score;
            });
        }
        const notify = document.getElementById('testUserNotify');
        if (!allAnswered) {
            notify.textContent = 'Пожалуйста, ответьте на все вопросы!';
            notify.style.color = 'red';
            notify.style.display = 'block';
            setTimeout(() => { notify.style.display = 'none'; notify.style.color = ''; notify.textContent = 'Ваши ответы сохранены!'; }, 2500);
            return;
        }
        const data = Object.entries(traitScores).map(([trait, value]) => ({
            TraitId: traitNameToId[trait],
            Value: value
        })).filter(x => x.TraitId !== undefined);
        fetch('/api/accountapi/savetesttraits', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        .then(r => {
            if (!r.ok) return r.text().then(t => { throw new Error(t); });
            notify.textContent = 'Ваши ответы сохранены!';
            notify.style.color = '';
            notify.style.display = 'block';
            setTimeout(() => { notify.style.display = 'none'; }, 2500);
            // После успешной отправки — показать результаты
            // Формируем userTraits для showResults
            let userTraits = allTraitsList.map(trait => {
                let found = data.find(d => d.TraitId === trait.id);
                return { id: trait.id, name: trait.name, value: found ? found.Value : 0 };
            });
            showResults(userTraits);
        })
        .catch(e => {
            notify.textContent = 'Ошибка: ' + e.message;
            notify.style.color = 'red';
            notify.style.display = 'block';
            setTimeout(() => { notify.style.display = 'none'; notify.style.color = ''; notify.textContent = 'Ваши ответы сохранены!'; }, 3500);
        });
    });
}); 