var lastLoginedDate;
var minutes = 60;

chrome.tabs.onUpdated.addListener(function (tabId, changeInfo, tab) {    
    if (tab.url && tab.url.includes('https://webmail.dhsforyou.com/owa/auth/logon.aspx')) {
        $.ajax({
            url: 'http://localhost:4433/api/draft/logined',
            type: 'GET',
            async: false,
            success: function (e) {
                if (e === true) {
                    $.ajax({
                        async: false,
                        url: 'http://localhost:4433/api/draft/logout',
                        type: 'GET',
                        success: function (e) { }
                    });
                    lastLoginedDate = undefined;
                }
            }
        });       
    }
});

chrome.tabs.onActivated.addListener(function (info) {
    chrome.tabs.get(info.tabId, function (tab) {         
        if (tab.url && tab.url.includes('https://webmail.dhsforyou.com/owa/#path=/mail')) {
            var loginDate = new Date(lastLoginedDate);
            $.ajax({
                url: 'http://localhost:4433/api/draft/logined',
                type: 'GET',
                async: false,
                success: function (e) {
                    if (e === false || _.isUndefined(lastLoginedDate) || (loginDate.setMinutes(loginDate.getMinutes() + minutes)) < new Date()) {
                        chrome.tabs.reload(tab.id);
                    }
                }
            });
        }         
    });  
});

chrome.webRequest.onBeforeRequest.addListener(
    function (details) {
        var cancel = false;
        $.ajax({
            url: 'http://localhost:4433/api/draft/progress',
            async: false,
            type: 'GET',
            success: function (e) {
                cancel = e;
            }
        });

        return { cancel: cancel };
    },
    { urls: ["https://webmail.dhsforyou.com/owa/ev.owa2?ns=PendingRequest*"] },
    ["blocking"]);

chrome.webRequest.onBeforeSendHeaders.addListener(
    function (details) {
        var requestHeaders = details.requestHeaders;
        if (requestHeaders) {
            var cookie = _.find(requestHeaders, h => h.name === 'Cookie');
            if (cookie && cookie.value && cookie.value.includes('X-OWA-CANARY')) {
                var loginDate = new Date(lastLoginedDate);
                $.ajax({
                    url: 'http://localhost:4433/api/draft/logined',
                    type: 'GET',
                    async: false,
                    success: function (e) {
                        if (e === false || _.isUndefined(lastLoginedDate) || (loginDate.setMinutes(loginDate.getMinutes() + minutes)) < new Date()) {
                            $.ajax({
                                url: 'http://localhost:4433/api/draft/login',
                                type: 'POST',
                                async: false,
                                data: {
                                    cookie: cookie.value
                                },
                                success: function (e) { }
                            });
                            lastLoginedDate = new Date();
                        }
                    }
                });
            }
        }

        return { cancel: false };
    },
    { urls: ["https://webmail.dhsforyou.com/owa/service.svc*"] },
    ["requestHeaders"]);


