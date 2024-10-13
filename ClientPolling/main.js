var EventSource = require('eventsource')

function longPoll() {
    fetch('http://localhost:5231/api/Update/longpoll')
        .then(response => response.json())
        .then(data => {
            // Process received update
            console.log(data);
            // Initiate next long poll request
            longPoll();
        })
        .catch(error => {
            console.error('Long poll request failed', error);
            // Retry long poll after a delay
            setTimeout(longPoll, 3000);
        });
}

function sse() {
    const eventSource = new EventSource('http://localhost:5231/api/Update/sse');

    eventSource.onmessage = function (event) {
        const item = JSON.parse(event.data);
        console.log("New item received:", item);
    };
}

sse();