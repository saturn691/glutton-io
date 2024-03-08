export enum ClientMsgType {
  Join,
  UpdatePosition,
}

export enum ServerMsgType {
  InitSocketId,
  PlayerJoined,
  PlayerLeft,
  UpdatePlayersPosition,
}

export type JoinMessageData = {
  playerId: string;
};
