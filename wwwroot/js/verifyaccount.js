$(function() {
    // Telegram уведомление
    $('#telegramForm').on('submit', function(e) {
        e.preventDefault();
        $('#telegramNotify').removeClass('d-none');
    });

    // Госуслуги: показать поле для кода и запустить таймер
    var timerInterval;
    $('#gosuslugiForm').on('submit', function(e) {
        e.preventDefault();
        var $phone = $('#gosuslugiPhone');
        var $codeGroup = $('#smsCodeGroup');
        var $timer = $('#timer');
        var $btn = $('#gosuslugiSendBtn');
        if ($codeGroup.hasClass('d-none')) {
            // Первый этап: отправка телефона
            $codeGroup.removeClass('d-none');
            $phone.prop('disabled', true);
            $btn.text('Подтвердить код');
            // Запуск таймера на 1 минуту
            var timeLeft = 60;
            $timer.text('01:00');
            timerInterval = setInterval(function() {
                timeLeft--;
                var min = Math.floor(timeLeft / 60);
                var sec = timeLeft % 60;
                $timer.text('0' + min + ':' + (sec < 10 ? '0' : '') + sec);
                if (timeLeft <= 0) {
                    clearInterval(timerInterval);
                    $timer.text('Время вышло');
                    $btn.prop('disabled', true);
                }
            }, 1000);
        } else {
            // Второй этап: подтверждение кода (заглушка)
            alert('Код подтверждён (заглушка)');
            $('#gosuslugiModal').modal('hide');
            clearInterval(timerInterval);
            // Сброс формы
            setTimeout(function() {
                $phone.prop('disabled', false);
                $btn.text('Получить код');
                $btn.prop('disabled', false);
                $codeGroup.addClass('d-none');
                $timer.text('');
                $('#gosuslugiForm')[0].reset();
            }, 500);
        }
    });
}); 