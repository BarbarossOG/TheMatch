$(function() {
    $('#closeAccountNotify').hide();
    $('#closeAccountForm').on('submit', function(e) {
        e.preventDefault();
        $('#closeAccountNotify').fadeIn(200);
        setTimeout(function() {
            $('#closeAccountNotify').fadeOut(400);
        }, 2500);
    });
}); 