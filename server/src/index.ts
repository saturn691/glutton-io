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

const simulate = (socket: WebSocket, game: GameState) => {
  let socketId = uuid.v4();
  game.AddPlayer(socket, socketId, { playerId: "test_opponent" });

  setInterval(() => {
    // Move player position 0.001 units to the right
    let prevPosition = game.players[socketId].position;

    game.UpdatePlayerPosition(socketId, {
      x: prevPosition.x + 0.1,
      y: prevPosition.y,
    });
  }, 100);
};

// Initialize DB connection
const main = async () => {
  config();
  // await connectToDB();

  const ws = new WebSocketServer({ port: 8080 });
  const game = new GameState(1, ws);

  //   simulate(null, game);

  ws.on("listening", () => {
    console.log("listening to ws connections on port 8080");
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
