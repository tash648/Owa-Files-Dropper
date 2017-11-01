chrome.runtime.onMessage.addListener(function (msg, sender) {
    if (msg.from === 'content') {        
        $.get('http://localhost:4433/api/draft/host?url=' + msg.url);
    }
});