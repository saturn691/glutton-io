import { Position } from "./Blob.js";
import { Player, PlayerColor } from "./Player.js";
import { JoinMessageData, ServerMsgType } from "./MessageType.js";
import { WebSocketServer, WebSocket } from "ws";

export class GameState {
  id: number;

  players: { [socketId: string]: Player };
  ws: WebSocketServer;
  numPlayers: number;

  updatedPositions: Map<string, Position>;

  // Player Position Update
  constructor(id: number, websocketConn: WebSocketServer) {
    this.id = id;
    this.players = {};
    this.ws = websocketConn;
    this.numPlayers = 0;
    this.updatedPositions = new Map<string, Position>();
  }

  InitPlayerJoined(socket: WebSocket, socketId: string) {
    socket.send(
      JSON.stringify({
        type: ServerMsgType.InitSocketId,
        data: {
          socketId: socketId,
          players: this.players,
        },
      })
    );
  }

  AddPlayer(socket: WebSocket, socketId: string, msgData: JoinMessageData) {
    console.log("Adding player with socket id: ", socketId);
    let playerId = msgData.playerId;

    this.players[socketId] = new Player(
      // playerId,
      PlayerColor.Red,
      socket,
      socketId,
      { x: 0, y: 0 }, // TODO: Initialize from game
      50,             // TODO: Initialize from game
      playerId        // TODO: Change to BlobId
    );

    this.numPlayers++;

    this.broadcast(
      {
        type: ServerMsgType.PlayerJoined,
        data: socketId,
      },
      socketId
    );
  }

  RemovePlayer(socketId: string) {
    console.log("Removing player with socket id: ", socketId);

    this.broadcast(
      {
        type: ServerMsgType.PlayerLeft,
        data: socketId,
      },
      socketId
    );

    delete this.players[socketId];
    this.numPlayers--;
  }

  UpdatePlayerPosition(socketId: string, data: Position) {
    console.log("Updating player pos: ", socketId, data);
    this.players[socketId].UpdatePosition(data);

    // Update only once every player's position is updated
    this.updatedPositions.set(socketId, data);

    let updateList = [];
    for (const [key, value] of this.updatedPositions) {
      updateList.push({ socketId: key, position: value });
    }
    this.broadcast({
      type: ServerMsgType.UpdatePlayersPosition,
      data: updateList,
    });
    this.updatedPositions = new Map<string, Position>();
  }

  broadcast(message: any, excludePlayerId?: string) {
    let jsonMsg = JSON.stringify(message);

    for (const socketId in this.players) {
      if (socketId != excludePlayerId && this.players[socketId] != null) {
        if (this.players[socketId].socket === null) continue;

        this.players[socketId].socket.send(jsonMsg);
      }
    }
  }
}
