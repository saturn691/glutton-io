import { config } from "dotenv";
import { WebSocketServer, WebSocket, RawData } from "ws";
import { GameState } from "../classes/Game.js";
import { ClientMsgType, ServerMsgType } from "../classes/MessageType.js";
import { DeletePlayersByGameId, connectToDB } from "../utils/db.js";

const testState = {};
let curPositionsUpdated = new Map();
let numPlayers = 0;
const MAX_PLAYERS_IN_ROOM = 250;

const testBroadcast = (msg: any, selfId: string) => {
  let count = 0;

  testState[selfId].socket.send(JSON.stringify(msg));
  for (const socketId in testState) {
    if (socketId == selfId) continue;
    if (count == MAX_PLAYERS_IN_ROOM - 1) break;
    count++;
    testState[socketId].socket.send(JSON.stringify(msg));
  }
};

const getNumVUs = () => {
  return Object.keys(testState).length;
};

export const handleTestWsMessage = (
  socket: WebSocket,
  socketId: string,
  msg: RawData,
) => {
  const msgJson = JSON.parse(msg.toString("utf8"));
  switch (msgJson.type) {
    case "join":
      numPlayers++;
      let id = numPlayers;
      console.log(`VU ${id} joined: `, socketId);
      testState[socketId] = { socket, id };
      socket.send(JSON.stringify({ type: "init", data: { socketId, id: id } }));
      break;

    case "playerEaten":
      // console.log(`VU ${testState[socketId].id} eaten`);
      testBroadcast({ type: "playerAte", data: { socketId } }, socketId);
      break;

    case "updatePosition":
      curPositionsUpdated.set(socketId, true);
      if (curPositionsUpdated.size == numPlayers) {
        testBroadcast(
          {
            type: "playerUpdatedPosition",
            data: { socketId, position: msgJson.data },
          },
          socketId,
        );
        curPositionsUpdated = new Map();
      }

      break;

    default:
      console.log("Unknown message type:", msgJson);
      break;
  }
};

export const handleTestRemovePlayer = (socketId: string) => {
  delete testState[socketId];
  numPlayers--;
  if (numPlayers == 0) {
    curPositionsUpdated = new Map();
  }
};
