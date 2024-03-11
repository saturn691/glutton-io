// import { Blob } from './blob.js';
// import { Entity } from './entity.js';
import { WebSocket } from "ws";
import { Blob, Position } from "./Blob.js";

export class Player {
  color: number; // 24-bit number representing the color of the player
  gameOver: boolean;
  socket: WebSocket;
  socketId: string; // SocketID uniquely identifies a player
  blob: Blob; // TODO: Define Player as group of Blobs (maybe through Dictionary?)

  constructor(
    color: number,
    socket: WebSocket,
    socketId: string,
    position: Position,
    size: number,
    blobId: string
  ) {
    //this.id = id;
    this.color = color;
    this.gameOver = false;
    this.socket = socket;
    this.socketId = socketId;
    this.blob = new Blob(blobId, position, size);
  }

  ToJson() {
    return {
      color: this.color,
      socketId: this.socketId,
      blob: this.blob.ToJson()
    }
  }

  UpdatePosition(position: Position) {
    // TODO: potentially change to UpdateState, and take size as parameter
    this.blob.position = position;
  }

  BlobEatsEnemy(appendedSize: number) {
    this.blob.EatEnemy(appendedSize);
  }
}
