// import { Blob } from './blob.js';
// import { Entity } from './entity.js';
import { WebSocket } from "ws";

export enum PlayerColor {
  Red,
}

export type Position = {
  x: number;
  y: number;
};

export class Player {
  id: string;
  color: PlayerColor;
  gameOver: boolean;
  socket: WebSocket;
  socketId: string;
  position: Position;

  constructor(
    id: string,
    color: PlayerColor,
    socket: WebSocket,
    socketId: string,
    position: Position
  ) {
    this.id = id;
    this.color = color;
    this.gameOver = false;
    this.socket = socket;
    this.socketId = socketId;
    this.position = position;
  }

  UpdatePosition(position: Position) {
    this.position = position;
  }
  // addBlob(blobID, x, y, size) {
  //     const blob = new Blob(blobID, x, y, size, this.color);
  //     this.blobs.push(blob);
  // }

  // // Remove blobs that are not alive
  // removeDeadBlobs() {
  //     this.blobs = this.blobs.filter(blob => blob.isAlive);
  // }

  // checkGameOver() {
  //     if (this.blobs.length === 0 || this.blobs.every(blob => !blob.isAlive)) {
  //         this.gameOver = true;
  //     }
  // }

  // // Render all blobs associated with the player
  // renderBlobs(context) {
  //     this.blobs.forEach(blob => {
  //         if (blob.isAlive) { // Only render if the blob is alive
  //             blob.render(context);
  //         }
  //     });
  // }
}
