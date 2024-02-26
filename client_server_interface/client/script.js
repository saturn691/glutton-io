import { io } from 'socket.io-client'

const messageInput = document.getElementById("message-input");
const form = document.getElementById("form");


const socket = io('http://localhost:5500')
socket.on('connect-off', () => {
    displayMessage(`You connected with ID: ${socket.id}`)
    socket.emit('connect', socket.id);
})

socket.on('receive-data', data_stream => {
    displayMessage(data_stream);
}
)


form.addEventListener("submit", e => {
    e.preventDefault();
    const data_stream = messageInput.value;

    if (data_stream === "") return;
    displayMessage(data_stream);
    socket.emit('send-data', data_stream);

    messageInput.value = "";
})



function displayMessage(message) {
    const div = document.createElement("div");
    div.textContent = message;
    document.getElementById("message-container").appendChild(div);
}