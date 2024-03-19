const fs = require('fs');

data = fs.readFileSync('results.json');
results = JSON.parse(data);

const ateLatencyKey = 'ateLatency';
const updatePosLatencyKey = 'updatePosLatency';

let ateTotal = 0;
let updatePosTotal = 0;
count = 0;
for (let key in results) {
    ateTotal += results[key][ateLatencyKey];
    updatePosTotal += results[key][updatePosLatencyKey];
    count++;
}

console.log(`Average update pos latency across ${count} VUs: `, updatePosTotal / count);
console.log(`Average ate latency across ${count} VUs: `, ateTotal / count);

