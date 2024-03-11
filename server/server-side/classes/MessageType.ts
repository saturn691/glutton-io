import { Position } from "./Blob.js";

export enum ClientMsgType {
  Join,
  UpdatePosition,
  PlayerEatenFood,
}

export enum ServerMsgType {
  InitSocketId,
  PlayerJoined,
  PlayerLeft,
  UpdatePlayersPosition,
  FoodAdded,
  PlayerAteFood,
  // BlobEaten,
}

export type JoinMessageData = {
  playerId: string,
  position: Position,
  size: number
};
