let host;

setHost = function (url) {
    host = url;
}

$(document).ready(function () {
    $('#loginPage').hide();
    $('#messagePage').hide();

    $.get('https://localhost:8080/api/draft/logined', (e) => {
        if (e === true) {
            $('#messagePage').show();

            $.get('https://localhost:8080/api/draft/message', a => {
                if (a === true) {
                    $('#openMessage').show();
                }
                else {
                    $('#openMessage').hide();
                }
            })
        }
        else {
            $('#loginPage').show();
        }
    });

    $('#login').click(e => {
        let email = $('#email').val();
        let password = $('#password').val();
        let host = window.location.host;

        $.post('https://localhost:8080/api/draft/login', {
            email: email,
            password: password,
            url: host
        }, (e) => {
            $('#loginPage').hide();
            $('#messagePage').show();

            $.get('https://localhost:8080/api/draft/message', a => {
                if (a === true) {
                    $('#openMessage').show();
                }
                else {
                    $('#openMessage').hide();
                }
            });
        });
    });

    $('#newMessage').click(e => {
        $.get('https://localhost:8080/api/draft/create', a => {
            if (a === 'OK') {
                $('#openMessage').show();
                $.get('https://localhost:8080/api/draft/open', url => {
                    if (url !== '') {
                        chrome.tabs.query({ currentWindow: true, active: true }, function (tabs) {
                            var activeTab = tabs[0];
                            chrome.tabs.sendMessage(activeTab.id, { message: "open", url: url });
                        });
                    }
                });
            }
            else {
                $('#openMessage').hide();
            }
        });
    });

    $('#openMessage').click(e => {
        $.get('https://localhost:8080/api/draft/open', url => {
            if (url !== '') {
                chrome.tabs.query({ currentWindow: true, active: true }, function (tabs) {
                    var activeTab = tabs[0];
                    chrome.tabs.sendMessage(activeTab.id, { message: "open", url: url });
                });
            }
            else {
                $('#openMessage').hide();
            }
        });
    });    

    $('#home').click(e => {
        $.get('https://localhost:8080/api/draft/home', url => {
            if (url !== '') {
                chrome.tabs.query({ currentWindow: true, active: true }, function (tabs) {
                    var activeTab = tabs[0];
                    chrome.tabs.sendMessage(activeTab.id, { message: "open", url: url });
                });
            }
        });
    });   

    $('#logout').click(e => {
        $.get('https://localhost:8080/api/draft/logout', url => {
            $('#loginPage').show();
            $('#messagePage').hide();

            $.get('https://localhost:8080/api/draft/home', url => {
                if (url !== '') {
                    chrome.tabs.query({ currentWindow: true, active: true }, function (tabs) {
                        var activeTab = tabs[0];
                        chrome.tabs.sendMessage(activeTab.id, { message: "open", url: url });
                    });
                }
            });
        });
    });   
});
