// save as: websocket-latency.js

module.exports = {

    sendMessagePeriodically: function(context, events, done) {
        // Function to send a message
        // Update position: 1
        const sendMessage = () => {
            context.ws.send(JSON.stringify({ type: 'load_test', message: "Hello!" }));
        };
        console.log("Sending message from VU");
        sendMessage();
        const interval = setInterval(sendMessage, 1000);

        // // Stop sending messages when the scenario is about to finish
        // context.on('done', function() {
        //     clearInterval(interval);
        // });

        return done();
    },

    sendMessage: function (context, events, done) {
        // Capture the send timestamp
        context.vars.sendTimestamp = new Date().getTime();

        // Send the message
        context.ws.send(JSON.stringify({"type": "load_test", "msg": "hello!"}));

        return done();
    },

    receiveMessage: function (context, events, done) {
        // Set up the message handler
        context.ws.on('message', function (message) {
        // Capture the receive timestamp
        let receiveTimestamp = new Date().getTime();
            
        // Calculate the latency
        let latency = receiveTimestamp - context.vars.sendTimestamp;
        console.log(`Send: ${context.vars.sendTimestamp}, Received: ${context.vars.receiveTimestamp}, latency: ${latency} ms`);

        // Optionally, perform additional actions here, e.g., checking the message content
        // Ensure this handler is only called once if expecting a single response
        context.ws.removeAllListeners('message');

        return done();
        });
    }
};