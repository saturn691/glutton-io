import { Position } from "./Blob.js";
import { Player } from "./Player.js";
import { Bot } from "./Bot.js";
import { FoodManager } from "./Food.js";
import { JoinMessageData, ServerMsgType } from "./MessageType.js";
import { WebSocketServer, WebSocket } from "ws";
import * as uuid from "uuid";
import { PlayerUtils } from "../utils/PlayerUtils.js";
import { getRandomValues } from "crypto";
import { DeletePlayersByGameId, insertPlayerIntoDB } from "../utils/db.js";

export class GameState {
  id: number;

  players: { [socketId: string]: Player };
  ws: WebSocketServer;
  numPlayers: number;

  // The size of the map. x and y are the maximum values for the map
  // e.g. if x = 100, then the map starts from (-50 ... 50)
  mapSize: Position;

  foodManager: FoodManager;

  updatedPositions: Map<string, Position>;

  playerStartingSize: number = 30;

  // Player Position Update
  constructor(id: number, websocketConn: WebSocketServer) {
    // Reset game table

    this.id = id;
    this.players = {};
    this.mapSize = { x: 7.5, y: 7.5 };
    this.foodManager = new FoodManager(1, 100, this.mapSize); // Default mass size
    this.ws = websocketConn;
    this.numPlayers = 0;
    this.updatedPositions = new Map<string, Position>();
  }

  /**
   * Called when a client first connects to the server. Sends generated socket
   * id for client & existing players state to client that connected.
   *
   * @param socket the player's socket
   * @param socketId the socketId of the player
   */
  InitPlayerJoined(socket: WebSocket, socketId: string) {
    console.log("New connection, socket id: ", socketId);
    let playersWithoutSocket = {};
    for (const socketId in this.players) {
      playersWithoutSocket[socketId] = this.players[socketId].ToJson();
    }

    socket.send(
      JSON.stringify({
        type: ServerMsgType.InitSocketId,
        data: {
          socketId: socketId,
          players: playersWithoutSocket,
          // foodBlobs: Object.fromEntries(this.foodManager.foodBlobs),
        },
      }),
    );
  }

  /**
   * 	Called when ClientMsgType.Join message is received. Essentially adds new
   * player to the players state and broadcasts ServerMsgType.PlayerJoined
   *
   * @param socket the player's socket
   * @param socketId the socketId of the player
   * @param msgData contains the playerId of the player used for display
   */
  async AddPlayer(
    socket: WebSocket,
    socketId: string,
    msgData: JoinMessageData,
  ) {
    console.log("Adding player: ", msgData);
    let playerId = "dummy_player_id"; // To change to input from user

    await insertPlayerIntoDB(this.id, socketId, playerId, msgData.size);

    let randomColor = Math.floor(
      Math.random() * (0xffffff - 0xaaaaaa) + 0xaaaaaa,
    );
    console.log(randomColor);

    let newPlayer = new Player(
      randomColor,
      socket,
      socketId,

      msgData.position, // TODO: assign position in connect
      msgData.size, // TODO: size - Initialize from game

      playerId, // TODO: Change to BlobId
    );

    this.players[socketId] = newPlayer;
    this.numPlayers++;

    this.Broadcast(
      {
        type: ServerMsgType.PlayerJoined,
        data: newPlayer.ToJson(),
      },
      socketId,
    );
  }

  /**
   * Called when client disconnects or leaves. Removes player from players
   * state and broadcasts ServerMsgType.PlayerLeft to all other players.
   *
   * @param socketId the socketId of the player to remove
   */
  RemovePlayer(socketId: string) {
    console.log("Removing player with socket id: ", socketId);

    this.Broadcast(
      {
        type: ServerMsgType.PlayerLeft,
        data: socketId,
      },
      socketId,
    );

    delete this.players[socketId];
    this.numPlayers--;
  }

  /**
   * Similar to AddPlayer, but for bots. Adds a bot to the players state and
   * broadcasts ServerMsgType.PlayerJoined
   */
  AddBot(name: string, size: number, position: Position) {
    const SIMULATE_INTERVAL_MS = 1 / 60;

    // Generate a unique socket id for the bot
    let socketId = uuid.v4();

    // Generate a random color
    let color = Math.floor(Math.random() * 0xffffff);

    // A null socket allows the game to not send messages to the bot
    let bot = new Bot(color, null, socketId, position, size, name);

    this.players[socketId] = bot;
    this.numPlayers++;

    this.Broadcast(
      {
        type: ServerMsgType.PlayerJoined,
        data: this.players[socketId],
      },
      socketId,
    );

    // Simulate bot movement
    setInterval(() => {
      // bot.Update(SIMULATE_INTERVAL_MS);
      bot.CheckCollisions(this);

      if (this.players[socketId] == null) return;

      PlayerUtils.HandleUpdatePlayerPosition(this, socketId, {
        x: bot.blob.position.x,
        y: bot.blob.position.y,
      });
    }, SIMULATE_INTERVAL_MS);
  }

  /**
   * Starts the asynchronous functions
   */
  Init() {
    this.GenerateFood();
  }

  private GenerateFood() {
    setInterval(() => {
      const foodBlob = this.foodManager.AddFoodBlob();
      this.Broadcast({
        type: ServerMsgType.FoodAdded,
        data: foodBlob,
      });
    }, 1000);
  }

  /**
   * Helper function to send a message to all players in game. Allows for
   * exclusion of message to 1 player which is useful when an update is
   * triggered by a particular client. For instance, if client C joins the game,
   * client A & B must be updated, but not client C
   *
   * @param message the message to broadcast
   * @param excludePlayerId the player to exclude from the broadcast
   */
  public Broadcast(message: any, excludePlayerId?: string) {
    let jsonMsg = JSON.stringify(message);

    for (const socketId in this.players) {
      if (socketId != excludePlayerId && this.players[socketId] != null) {
        if (this.players[socketId].socket === null) continue;
        this.players[socketId].socket.send(jsonMsg);
      }
    }
  }
}
