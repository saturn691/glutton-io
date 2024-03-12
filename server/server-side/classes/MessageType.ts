import { Position } from "./Blob.js";

export enum ClientMsgType {
  Join,
  UpdatePosition,
  PlayerEatenFood,
  PlayerEatenEnemy,
}

export enum ServerMsgType {
  InitSocketId,
  PlayerJoined,
  PlayerLeft,
  UpdatePlayersPosition,
  FoodAdded,
  PlayerAteFood,
  PlayerAteEnemy,
  // BlobEaten,
}

export type JoinMessageData = {
  playerId: string;
  position: Position;
  size: number;
};
