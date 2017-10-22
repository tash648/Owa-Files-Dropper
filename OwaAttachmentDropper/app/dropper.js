
let connection = $.hubConnection('https://localhost:8080/signalr', { useDefaultPath: false });
let watcherHub = connection.createHubProxy('fileSystemWatcherHub');

watcherHub.on('AttachFile', function () {
    if (window.location.href.indexOf("ItemID") !== -1) {
        window.location.reload();
    }    
});

let tryingToReconnect = false;
let reconnect;

connection.reconnecting(() => {
    tryingToReconnect = true;
});

connection.disconnected(() => {
    tryingToReconnect = false;
    if (connection.lastError) {
        console.log("Disconnected. Reason: " + connection.lastError.message);
    }
    if (!_.isUndefined(reconnect)) {
        clearInterval(reconnect);
    }

    reconnect = setInterval(function () {
        connection.start().done(function () {
            clearInterval(reconnect);
            $.get('https://localhost:8080/api/draft/host?url=' + window.location.origin);
            console.log('re connectionId= ' + connection.id);
        });
    }, 5000);
});

connection.start().done(function () {
    $.get('https://localhost:8080/api/draft/host?url=' + window.location.origin);
    console.log('connectionId= ' + connection.id);
}); 

chrome.runtime.onMessage.addListener(
    function (request, sender, sendResponse) {
        if (request.message === "open") {
            let url = request.url;
            window.location.href = url;
        }
    }
);