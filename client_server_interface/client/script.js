import { io } from 'socket.io-client'

const testingButton = document.getElementById("testing-button");
const messageInput = document.getElementById("message-input");
const form = document.getElementById("form");

const socket = io('http://localhost:5500')
socket.on('connect', () => {
    displayMessage(`You connected with ID: ${socket.id}`)
    socket.emit('connect', socket.id);
})

socket.on('receive-data', data_stream => {
    displayMessage(data_stream);
})

// Define an array to store RTT data
let rttData = [];

// Function to save RTT data to a CSV file
function saveRTTDataToCSV() {
    // Create a CSV content string
    const csvContent = "RTT (ms),Timestamp (ms)\n" + rttData.map(({ rtt, timestamp }) => `${rtt},${timestamp}`).join("\n");

    // Create a Blob containing the CSV data
    const blob = new Blob([csvContent], { type: 'text/csv' });

    // Create a temporary anchor element to trigger the download
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = 'rtt_data.csv';

    // Append the anchor to the body and trigger the download
    document.body.appendChild(link);
    link.click();

    // Clean up
    document.body.removeChild(link);
}

socket.on('receive-data-rtt', (data) => {
    const currentTime = Date.now(); // Current time when message is received
    const rtt = currentTime - data.timestamp; // Calculate RTT
    rttData.push({ rtt, timestamp: data.timestamp });
    displayMessage(`RTT: ${rtt}ms - Received datastream: ${data.data}`);
});


form.addEventListener("submit", e => {
    e.preventDefault();
    const data_stream = messageInput.value;

    if (data_stream === "") return;
    displayMessage(data_stream);
    socket.emit('send-data', data_stream);

    messageInput.value = "";
})

testingButton.addEventListener("click", e => {
    // Simulate sending 20 random values with delays between each send
    const randomValues = Array.from({ length: 200 }, () => Math.floor(Math.random() * 100));
    let index = 0;
    const intervalId = setInterval(() => {
        if (index < randomValues.length) {
            const randomValue = randomValues[index++];
            const data_stream = `${randomValue}`;
            const timestamp = Date.now();
            displayMessage(data_stream);
            socket.emit('send-data-rtt', { data: data_stream, timestamp });
        } else {
            clearInterval(intervalId);
            saveRTTDataToCSV();
        }
    }, 1000); // Adjust the interval (in milliseconds) between each send
})

function displayMessage(message) {
    const div = document.createElement("div");
    div.textContent = message;
    document.getElementById("message-container").appendChild(div);
}