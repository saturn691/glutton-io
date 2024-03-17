// save as: websocket-latency.js
const fs = require('fs');
const lastUpdatePosSendTime = "lastUpdatePosSendTime";
const lastAteSendTime = "lastAteSendTime"; 

const ateLatencyKey = 'ateLatency';
const updatePosLatencyKey = 'updatePosLatency';

const eatingInterval = 500;
const updatePosInterval = 10;

const handleUpdatedPosition = (context, msgData, isAteType) => {
    let socketId = msgData.socketId;
    if (socketId == context.selfSocketId) {
        const currentTime = new Date().getTime();
        const lastSendTime = isAteType ? context[lastAteSendTime] : context[lastUpdatePosSendTime];

        const latency = currentTime - lastSendTime;
        
        let latencyKey = isAteType ? ateLatencyKey : updatePosLatencyKey;
        if (context[latencyKey] == null) {
            context[latencyKey] = {
                count: 0,
                total: 0
            }
        }

        context[latencyKey].count++;
        context[latencyKey].total += latency;
        const averageLatency = context[latencyKey].total / context[latencyKey].count;

        console.log(`VU ${context.selfId}\t Average ${latencyKey}: ${averageLatency.toFixed(3)} ms`);
        // if (latencyKey == ateLatencyKey) {
        // }


        let resultsJson = fs.readFileSync('results.json');

        let results = JSON.parse(resultsJson);
        if (!results[context.selfId]) {
            results[context.selfId] = {};
            results[context.selfId][ateLatencyKey] = 0;
            results[context.selfId][updatePosLatencyKey] = 0;
        }

        results[context.selfId][latencyKey] = averageLatency;
        fs.writeFileSync('results.json', JSON.stringify(results, null, 4));
        return;
    }
}

module.exports = {


    sendJoinMsg: function(context, events, done) {
        context.ws.send(JSON.stringify({"type": "join", "msg": "hello!"}));
        return done();
    },

    
    simulate: function(context, events, done) {
        const sendUpdatePos = () => {
            context[lastUpdatePosSendTime] = new Date().getTime();
            context.ws.send(JSON.stringify({ type: 'updatePosition', message: "random" }));
        };

        const simulateEating = () => {
            context[lastAteSendTime] = new Date().getTime();
            context.ws.send(JSON.stringify({ type: 'playerEaten', message: "random" }));
        }
        
        
        setTimeout(() => {
            context.vars.eatInterval = setInterval(simulateEating, eatingInterval);
            context.vars.updatePosInterval = setInterval(sendUpdatePos, updatePosInterval);
        }, 1000);
        done();
    },

    

    receiveMessage: function (context, events, done) {
        
        context.ws.on('message', function (msg) {
            let msgJson = JSON.parse(msg);
            const msgData = msgJson.data;

            switch (msgJson.type) {
                case 'init': 
                    context.selfSocketId = msgData.socketId;
                    context.selfId = msgData.id;
                    break;
                case 'playerUpdatedPosition': 
                    handleUpdatedPosition(context, msgData, false);
                    break;
                case 'playerAte': 
                    handleUpdatedPosition(context, msgData, true);
                    break;
            }
        });

        return done();
    }
    
};