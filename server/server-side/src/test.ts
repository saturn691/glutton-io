import { config } from "dotenv";
import { WebSocketServer, WebSocket, RawData } from "ws";
import { GameState } from "../classes/Game.js";
import { ClientMsgType, ServerMsgType } from "../classes/MessageType.js";
import { DeletePlayersByGameId, connectToDB } from "../utils/db.js";

const testState = {};
const MAX_PLAYERS_IN_ROOM = 250;

const testBroadcast = (msg: any, selfId: string, excludeId?: string) => {
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
      let id = getNumVUs() + 1;
      console.log(`VU ${id} joined: `, socketId);
      testState[socketId] = { socket, id };
      socket.send(JSON.stringify({ type: "init", data: { socketId, id: id } }));
      // testBroadcast({ type: "playerJoined", data: { socketId } }, socketId);
      break;

    case "playerEaten":
      // console.log(`VU ${testState[socketId].id} eaten`);
      testBroadcast({ type: "playerAte", data: { socketId } }, socketId);
      break;

    case "updatePosition":
      // console.log(`VU ${testState[socketId].id} updated position`);
      testBroadcast(
        {
          type: "playerUpdatedPosition",
          data: { socketId, position: msgJson.data },
        },
        socketId,
      );
      break;

    default:
      console.log("Unknown message type:", msgJson);
      break;
  }
};

export const handleTestRemovePlayer = (socketId: string) => {
  delete testState[socketId];
};
