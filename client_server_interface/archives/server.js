const { instrument } = require("@socket.io/admin-ui");
const io = require("socket.io")(5500, {
    cors: {
        origin: ["http://localhost:8081", "http://localhost:8080", "https://admin.socket.io"], // bypass issue with CORS
        credentials: true
    },
}); // define socket on which server is running

io.on("connection", socket => {
    console.log(socket.id);
    socket.on('send-data', (data_stream) => {
        socket.broadcast.emit('receive-data', `Received datastream: ${data_stream}`); // send to all clients except sender
    }); // takes same arguments as client
    socket.on('send-data-rtt', data => {
        socket.broadcast.emit('receive-data', `received datastream: ${data.data}`); // send to all clients except sender
        io.to(socket.id).emit('receive-data-rtt', data);
    })
}) // runs every time a client connects to server and give socket instance to each one


instrument(io, {
    auth: false,
    namespace: "/admin"
});

