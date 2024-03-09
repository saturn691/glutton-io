export enum ClientMsgType {
  Join,
  UpdatePosition,
}

export enum ServerMsgType {
  InitSocketId,
  PlayerJoined,
  PlayerLeft,
  UpdatePlayersPosition,
  BlobEats,
  BlobIsEaten,
}

export type JoinMessageData = {
  playerId: string;
};
