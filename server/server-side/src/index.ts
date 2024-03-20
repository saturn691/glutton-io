// import { connectToDB } from '../db.js'; // Assuming you have a separate file for DB connection
import { config } from "dotenv";
import { WebSocketServer, WebSocket, RawData } from "ws";
import { GameState } from "../classes/Game.js";
import { ClientMsgType, ServerMsgType } from "../classes/MessageType.js";
import { connectToDB, deletePlayersByGameId } from "../utils/db.js";

import * as uuid from "uuid";

import { PlayerUtils } from "../utils/PlayerUtils.js";
import { handleTestRemovePlayer, handleTestWsMessage } from "./test.js";

const timeout = async (ms: number) => {
  return new Promise((res, rej) => setTimeout(() => res(true), ms));
};

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
        PlayerUtils.HandleUpdatePlayerPosition(game, socketId, msgJson.data);
        break;

      case ClientMsgType.PlayerEatenFood:
        PlayerUtils.HandlePlayerEatenFood(game, socketId, msgJson.data);
        break;

      case ClientMsgType.PlayerEatenEnemy:
        PlayerUtils.HandlePlayerEatenEnemy(game, socketId, msgJson.data);
        break;
      case ClientMsgType.PlayerThrewMass:
        PlayerUtils.HandlePlayerThrewMass(game, socketId, msgJson.data);
        break;
      default:
        console.log("Unknown message type:", msgJson);
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
  // Add a small bot to the left
  game.AddBot("smallBot", 10, { x: -10, y: 0 });
  // Add a big bot to the right
  game.AddBot("bigBot", 100, { x: 10, y: 0 });
};

// Initialize DB connection
const main = async () => {
  const PORT = 8080;
  config();
  await connectToDB();

  const ws = new WebSocketServer({ port: PORT });

  let gameId = 1;
  await deletePlayersByGameId(gameId);
  const game = new GameState(gameId, ws);
  game.GenerateFood();
  // simulate(game);

  ws.on("listening", () => {
    console.log(`listening to ws connections on port ${PORT}`);
  });

  ws.on("connection", async (socket) => {
    const socketId = uuid.v4();

    // For testing
    // socket.on("message", (msg) => handleTestWsMessage(socket, socketId, msg));

    // For actual game
    game.InitPlayerJoined(socket, socketId);
    socket.on("message", (msg) => handleWsMessage(game, socket, socketId, msg));

    socket.on("close", () => {
      // handleTestRemovePlayer(socketId);
      game.RemovePlayer(socketId);
    });

    socket.on("error", (err) => {
      // handleTestRemovePlayer(socketId);
      game.RemovePlayer(socketId);
    });
  });
};

main();
