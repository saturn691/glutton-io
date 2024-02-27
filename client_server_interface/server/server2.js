import { WebSocketServer } from 'ws';
import { connectToDB } from './db.js'; // Assuming you have a separate file for DB connection
import { config } from 'dotenv';

// Initialize DB connection
const main = async() => {
    config();
    await connectToDB();

    const ws = new WebSocketServer({port: 8080})

    ws.on('listening', () => {
        console.log('listening to ws connections on port 8080')
    })

    ws.on('connection', (socket) => {
        
        
        socket.on('message', (data) => {
            const utf8Data = data.toString('utf8');
            try {
                const jsonData = JSON.parse(utf8Data);
                console.log("Received: ", jsonData)
            } catch (error) {
                console.error('Error parsing JSON:', error);
            }

            
            socket.send(`Received message!`)

            setInterval(() => {
                socket.send('Message from server');
            }, 5000);
            
        })


        socket.on('close', () => {
            console.log('closed')
        })


        socket.on('error', (err) => {
            console.log(err)
        })
    })
}

main();