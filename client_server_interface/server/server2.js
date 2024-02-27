import {WebSocketServer} from 'ws'

const sockserver = new WebSocketServer({port: 8080})

sockserver.on('connection', (socket) => {
    console.log('connected')
    socket.on('message', (data) => {
        console.log(data)
        socket.send(`Received datastream: ${data}`)
    })
    socket.on('close', () => {
        console.log('closed')
    })
    socket.on('error', (err) => {
        console.log(err)
    })
})

