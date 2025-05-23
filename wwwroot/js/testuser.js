$(function() {
    $('#testUserForm').on('submit', function(e) {
        e.preventDefault();
        let allAnswered = true;
        for (let i = 1; i <= 12; i++) {
            if (!$('input[name="q' + i + '"]:checked').val()) {
                allAnswered = false;
                break;
            }
        }
        if (!allAnswered) {
            $('#testUserNotify').text('Пожалуйста, ответьте на все вопросы!').css('color', 'red').fadeIn(200);
            setTimeout(function() {
                $('#testUserNotify').fadeOut(400, function() {
                    $(this).css('color', '');
                    $(this).text('Ваши ответы сохранены!');
                });
            }, 2500);
            return;
        }
        // Здесь будет отправка данных в будущем
        $('#testUserNotify').text('Ваши ответы сохранены!').css('color', '').fadeIn(200);
        setTimeout(function() {
            $('#testUserNotify').fadeOut(400);
        }, 2500);
    });
}); 