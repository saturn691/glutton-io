import { Position } from "./Blob.js";

export enum ClientMsgType {
  Join,
  UpdatePosition,
  PlayerEatenFood,
  PlayerEatenEnemy,
  PlayerThrewMass,
}

export enum ServerMsgType {
  InitSocketId,
  PlayerJoined,
  PlayerLeft,
  UpdatePlayersPosition,
  FoodAdded,
  PlayerAteFood,
  PlayerAteEnemy,
  PlayerThrewMass,
}

export type JoinMessageData = {
  playerId: string;
  position: Position;
  size: number;
};
