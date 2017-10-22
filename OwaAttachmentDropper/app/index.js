
$(document).ready(function() {
    $('#messagePage').hide();
    $('#login').click(e => {
        let email = $('#email').val();
        let password = $('#password').val();
    
        $.post('https://localhost:8080/api/draft/login', {
            email: email,
            password: password
        }, (e) => {
            $('#loginPage').hide();
            $('#messagePage').show();
        });
    });
});
