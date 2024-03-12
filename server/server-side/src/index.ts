// import { connectToDB } from '../db.js'; // Assuming you have a separate file for DB connection
import { config } from "dotenv";
import { WebSocketServer, WebSocket, RawData } from "ws";
import { GameState } from "../classes/Game.js";
import { ClientMsgType, ServerMsgType } from "../classes/MessageType.js";

import * as uuid from "uuid";

import { PlayerUtils } from "../utils/PlayerUtils.js";

const handleWsMessage = (
  game: GameState,
  socket: WebSocket,
  socketId: string,
  msg: RawData
) => {
  try {
    const msgJson = JSON.parse(msg.toString("utf8"));

    if (msgJson.type == ClientMsgType.Join) {
      game.AddPlayer(socket, socketId, msgJson.data);
      return;
    }

    if (!game.players[socketId]) {
      return;
    }

    switch (msgJson.type) {
      case ClientMsgType.UpdatePosition:
        // game.UpdatePlayerPosition(socketId, msgJson.data);
        PlayerUtils.HandleUpdatePlayerPosition(game, socketId, msgJson.data);
        break;

      case ClientMsgType.PlayerEatenFood:
        PlayerUtils.HandlePlayerEatenFood(game, socketId, msgJson.data);
        break;

      case ClientMsgType.PlayerEatenEnemy:
        console.log("Player ate enemy");
        PlayerUtils.HandlePlayerEatenEnemy(game, socketId, msgJson.data);
        break;

      default:
        console.log("Unknown message type:", msgJson.type);
        break;
    }
  } catch (error) {
    console.error("Error parsing JSON:", error);
  }
};


/**
 * Function for testing purposes. Simulates a game state. Change this function
 * to simulate different game states.
 * @param game the game state to modify
 */
const simulate = (game: GameState) => {
  // // Add a small bot to the left
  // game.AddBot(
  //   "smallBot",
  //   10,
  //   {x : -10 , y : 0}
  // );

  // Add a big bot to the right
  // game.AddBot(
  //   "bigBot",
  //   100,
  //   {x : 10 , y : 0}
  // );

  game.Init();
};

// Initialize DB connection
const main = async () => {
  const PORT = 8080;
  config();

  const ws = new WebSocketServer({ port: PORT });
  const game = new GameState(1, ws);

  simulate(game);

  ws.on("listening", () => {
    console.log(`listening to ws connections on port ${PORT}`);
  });

  ws.on("connection", async (socket) => {
    const socketId = uuid.v4();
    game.InitPlayerJoined(socket, socketId);

    // setTimeout(() => {
    //   socket.close();
    // }, 2000);

    socket.on("message", (msg) => handleWsMessage(game, socket, socketId, msg));

    socket.on("close", () => {
      console.log("Socket closed");
      game.RemovePlayer(socketId);
    });

    socket.on("error", (err) => {
      console.error("Socket error:", err);
      game.RemovePlayer(socketId);
    });
  });
};

main();
