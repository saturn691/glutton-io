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
  BlobEaten,
}

export type JoinMessageData = {
  playerId: string;
};
