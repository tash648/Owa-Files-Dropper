let host;

setHost = function (url) {
    host = url;
}

$(document).ready(function () {

    $('#wrong').hide();    
    $('#notWork').hide();
    $('#logouting').hide();
    $('#logging').hide();
    $('#loginPage').hide();
    $('#messagePage').hide();

    $('#loading').show();
    $.ajax({
        url: 'http://localhost:4433/api/draft/logined',
        type: 'GET',
        success: function (e) {
            if (e === false) {
                $('#wrong').hide();
                $('#logging').hide();
                $('#loginPage').show();
                $('#messagePage').hide();
            }
            else {
                $('#messagePage').show();
            }
        },
        error: function () {
            $('#notWork').show();
        },
        complete: function () {
            $('#loading').hide();
        }
    });

    $('#login').click(e => {

        $('#wrong').hide();  
        $('#logging').show();

        $("#login").attr('disabled', true);

        let email = $('#email').val();
        let password = $('#password').val();
        let host = window.location.host;

        $.ajax({
            url: 'http://localhost:4433/api/draft/login',
            type: 'POST',
            data: {
                email: email,
                password: password
            },
            success: function (e) {
                if (e === true) {
                    $('#loginPage').hide();
                    $('#messagePage').show();

                    window.close();
                }
                else {
                    $('#wrong').show();
                }
            },
            complete: function () {
                $('#logging').hide();
                $("#login").attr('disabled', false);
            }
        });
    });  

    $('#logout').click(e => {
        $('#logouting').show();
        $("#logout").attr('disabled', true);
        $.ajax({
            url: 'http://localhost:4433/api/draft/logout',
            type: 'GET',
            success: function (e) {
                if (e === '') {
                    $('#wrong').hide();
                    $('#logging').hide();
                    $('#loginPage').show();
                    $('#messagePage').hide();
                }
            },
            complete: function () {
                $('#logouting').hide();
                $("#logout").attr('disabled', false);
            }
        });
    });   
});
