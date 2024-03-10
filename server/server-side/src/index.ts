// import { connectToDB } from '../db.js'; // Assuming you have a separate file for DB connection
import { config } from "dotenv";
import { WebSocketServer, WebSocket, RawData } from "ws";
import { GameState } from "../classes/Game.js";
import { ClientMsgType, ServerMsgType } from "../classes/MessageType.js";
import * as uuid from "uuid";


const handleWsMessage = (
  game: GameState,
  socket: WebSocket,
  socketId: string,
  msg: RawData
) => {
  try {
    const msgJson = JSON.parse(msg.toString("utf8"));
    switch (msgJson.type) {
      case ClientMsgType.Join:
        game.AddPlayer(socket, socketId, msgJson.data);
        break;
      case ClientMsgType.UpdatePosition:
        game.UpdatePlayerPosition(socketId, msgJson.data);
        break;
      default:
        console.log("Unknown message type:", msgJson.type);
        break;
    }
  } catch (error) {
    console.error("Error parsing JSON:", error);
  }
};


const simulate = (game: GameState, interval: number) => {
  setInterval(() => {
    game.AddBot();
  }, interval);
}


// Initialize DB connection
const main = async () => {
  const PORT = 8080;
  config();

  const ws = new WebSocketServer({ port: PORT });
  const game = new GameState(1, ws);

  simulate(game, 5000);

  ws.on("listening", () => {
    console.log(`listening to ws connections on port ${PORT}`);
  });

  ws.on("connection", async (socket) => {
    const socketId = uuid.v4();
    game.InitPlayerJoined(socket, socketId);

    socket.on("message", (msg) => handleWsMessage(game, socket, socketId, msg));

    socket.on("close", () => {
      game.RemovePlayer(socketId);
    });

    socket.on("error", (err) => {
      game.RemovePlayer(socketId);
    });
  });
};


main();
