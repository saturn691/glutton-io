import { Position } from "./Blob.js";
import { Player } from "./Player.js";
import { Blob } from "./Blob.js";
import { Bot } from "./Bot.js";
import { Food, FoodManager } from "./Food.js";
import { JoinMessageData, ServerMsgType } from "./MessageType.js";
import { WebSocketServer, WebSocket } from "ws";
import * as uuid from "uuid";
import { set } from "firebase/database";


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

  // Player Position Update
  constructor(id: number, websocketConn: WebSocketServer) {
    this.id = id;
    this.players = {};
    this.mapSize = { x: 30, y: 30} ;
    this.foodManager = new FoodManager(10, 100, this.mapSize);
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


  /**
   * 	Called when ClientMsgType.Join message is received. Essentially adds new 
   * player to the players state and broadcasts ServerMsgType.PlayerJoined
   * 
   * @param socket the player's socket 
   * @param socketId the socketId of the player
   * @param msgData contains the playerId of the player used for display
   */
  AddPlayer(socket: WebSocket, socketId: string, msgData: JoinMessageData) {
    console.log("Adding player with socket id: ", socketId);
    let playerId = msgData.playerId;

    this.players[socketId] = new Player(
      parseInt("FF0000", 16),
      socket,
      socketId,
      { x: 0, y: 0 }, // TODO: Initialize from game
      50,             // TODO: Initialize from game
      playerId        // TODO: Change to BlobId
    );

    this.numPlayers++;

    this.Broadcast(
      {
        type: ServerMsgType.PlayerJoined,
        data: this.players[socketId],
      },
      socketId
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
      socketId
    );

    delete this.players[socketId];
    this.numPlayers--;
  }


  /**
   * Similar to AddPlayer, but for bots. Adds a bot to the players state and
   * broadcasts ServerMsgType.PlayerJoined
   */
  AddBot() {
    const BOT_NAME = "bot";
    const STARTING_SIZE = 50;
    const SIMULATE_INTERVAL_MS = 100;
    const BOT_SPEED = 0.05;

    // Generate a unique socket id for the bot
    let socketId = uuid.v4();

    // A null socket allows the game to not send messages to the bot
    let bot = new Bot(
      parseInt("00FF00", 16),
      null,
      socketId,
      { x: 5, y: 0 }, // TODO: Initialize from game
      STARTING_SIZE,  // TODO: Initialize from game
      BOT_NAME        // TODO: Change to BlobId
    );

    this.players[socketId] = bot;
    this.numPlayers++;

    this.Broadcast(
      {
        type: ServerMsgType.PlayerJoined,
        data: this.players[socketId],
      },
      socketId
    );

    // Simulate bot movement
    setInterval(() => {
      bot.Update(BOT_SPEED);

      this.UpdatePlayerPosition(socketId, {
        x: bot.blob.position.x,
        y: bot.blob.position.y,
      });
    }, SIMULATE_INTERVAL_MS);
  }


  /**
   * Called when ClientMsgType.UpdatePosition is received. Updates new position 
   * for client in players state. Once it has received an update from all 
   * different players, it will broadcast ServerMsgType.UpdatePlayersPosition 
   * so that clients can update their local state.
   * 
   * @param socketId the socketId of the player to update 
   * @param data the new position of the player
   */
  UpdatePlayerPosition(socketId: string, data: Position) {
    // console.log("Updating player pos: ", socketId, data);

    this.players[socketId].UpdatePosition(data);

    // Update only once every player's position is updated
    this.updatedPositions.set(socketId, data);

    let updateList = [];
    for (const [key, value] of this.updatedPositions) {
      updateList.push({ socketId: key, position: value });
    }
    this.Broadcast({
      type: ServerMsgType.UpdatePlayersPosition,
      data: updateList,
    });
    this.updatedPositions = new Map<string, Position>();
  }


  /**
   * Starts the asynchronous functions
   */
  Init() {
    // Starts food generation
    setInterval(() => {
      this.foodManager.AddFood();
    }, 1000);
    setInterval(() => {
      this.Update();
    }, 60);
  }


  /**
   * Main game loop. Must be called every frame to update the game state. 
   * Detects collisions and updates foods, viruses.
   */
  private Update() {
    // TODO - Add food (random in time and position)
    this.CheckFoodEaten();
    // TODO - Add viruses (random in time and position)
    this.CheckCollision();
    return;
  }

  private CheckFoodEaten() {
    for (const socketId in this.players) {
      if (this.players[socketId] != null) {
        let player = this.players[socketId];
        // TODO change once there are more blobs
        let playerBlob : Blob = player.blob;

        for (let i = 0; i < this.foodManager.food.length; i++) {
          let food : Food = this.foodManager.food[i];
          let response = Blob.WhoAteWho(playerBlob, food);
          if (response != 0)
          {
            console.log("Player ate food");
            playerBlob.EatEnemy(food.size);
            this.foodManager.RemoveFood(food.id);
          
            player.socket.send(
              JSON.stringify({
                type: ServerMsgType.BlobEats,
                data: playerBlob.size
              })
            )
          }
        }
      }
    }
  }


  // TODO very inefficient
  private CheckCollision() {
    for (const socketId in this.players) {
      if (this.players[socketId] != null) {
        let player = this.players[socketId];
        let playerBlob : Blob = player.blob;

        for (const enemySocketId in this.players) {
          if (this.players[enemySocketId] != null && enemySocketId != socketId) {
            let enemy = this.players[enemySocketId];
            let enemyBlob : Blob = enemy.blob;
            let response = Blob.WhoAteWho(playerBlob, enemyBlob);
            if (response == 1)
            {
              console.log(`${socketId} ate ${enemySocketId}`);
              playerBlob.EatEnemy(enemyBlob.size);
              this.RemovePlayer(enemySocketId);
            }
            else if (response == 2)
            {
              console.log(`${enemySocketId} ate ${socketId}`);
              enemyBlob.EatEnemy(playerBlob.size);
              this.RemovePlayer(socketId);
            }
          }
        }
      }
    }
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
  private Broadcast(message: any, excludePlayerId?: string) {
    let jsonMsg = JSON.stringify(message);

    for (const socketId in this.players) {
      if (socketId != excludePlayerId && this.players[socketId] != null) {
        if (this.players[socketId].socket === null) continue;
        
        console.log("Sending message to: ", socketId);
        this.players[socketId].socket.send(jsonMsg);
      }
    }
  }
}
