var version = "1.0";
var tabs = [];
var sendCookieInProgress = false;
var attachInProgress = false;

chrome.tabs.onUpdated.addListener(function (tabId, changeInfo, tab) {
    if (tab.url && tab.url.includes('https://webmail.dhsforyou.com/owa/#path=/mail')) {
        if (attachInProgress) {
            return;
        }
        attachInProgress = true;
        $.ajax({
            url: 'http://localhost:4433/api/draft/logined',
            type: 'GET',
            success: function (e) {
                if (e === false) {
                    var tabIdLocal = tabId;
                    var cookie;

                    if (tabs[tabId] === undefined) {
                        tabs[tabId] = tabId;
                        chrome.debugger.attach({
                            tabId: tabId
                        }, version, function (tabId) {
                            onAttach(tabIdLocal, cookie);
                        });
                        chrome.tabs.get(tabIdLocal, function (tab) {
                            if (tab.url && tab.url.includes('https://webmail.dhsforyou.com/owa/#path=/mail')) {
                                chrome.tabs.reload(tab.id);
                            }
                        });
                    }                                        
                }
            },
            complete: function () {
                attachInProgress = false;
            },
            error: function () {
                attachInProgress = false;
            }
        });

        return;
    }     

    if (tab.url && tab.url.includes('https://webmail.dhsforyou.com/owa/auth/logon.aspx')) {
        $.ajax({
            url: 'http://localhost:4433/api/draft/logout',
            type: 'GET',
            success: function (e) {
                tabs[tab.id] = undefined; 
            }
        });
    }
});

chrome.tabs.onActivated.addListener(function (info) {
    chrome.tabs.get(info.tabId, function (tab) {         
        if (tab.url && tab.url.includes('https://webmail.dhsforyou.com/owa/#path=/mail')) {
            $.ajax({
                url: 'http://localhost:4433/api/draft/logined',
                type: 'GET',
                success: function (e) {
                    if (e === false) {
                        chrome.tabs.reload(tab.id);
                    }
                }
            });
        }         
    });  
         
});

function allEventHandler(cookie, debuggeeId, message, params) {
    if (params && params.response && params.response.requestHeaders && params.response.requestHeaders.Cookie && params.response.requestHeaders.Cookie.includes('X-OWA-CANARY')) {
        var requestInfo = params.response;
        var tabId = debuggeeId.tabId;

        if (!sendCookieInProgress) {
            sendCookieInProgress = true;

            $.ajax({
                url: 'http://localhost:4433/api/draft/logined',
                type: 'GET',
                success: function (e) {
                    if (e === false) {
                        chrome.debugger.sendCommand({
                            tabId: debuggeeId.tabId
                        }, "Network.getResponseBody", {
                                "requestId": params.requestId
                            }, function (response) {
                                $.ajax({
                                    url: 'http://localhost:4433/api/draft/login',
                                    type: 'POST',
                                    data: {
                                        cookie: requestInfo.requestHeaders.Cookie
                                    },
                                    success: function (e) {
                                        chrome.debugger.detach({
                                            tabId: tabId
                                        });

                                        tabs[tabId] = undefined;
                                    },
                                    complete: function () {
                                        sendCookieInProgress = false;
                                    },
                                    error: function () {
                                        sendCookieInProgress = false;
                                    }
                                });
                            });
                    }
                }
            });  
        }                 
    }
}

function onAttach(tabId, cookie) {
    chrome.debugger.sendCommand({
        tabId: tabId
    }, "Network.enable");
    chrome.debugger.onEvent.addListener(function (debuggeeId, message, params) {
        allEventHandler(cookie, debuggeeId, message, params);
    });
}

