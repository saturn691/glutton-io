import { Position } from "../classes/Blob.js";
import { FoodManager } from "../classes/Food.js";
import { GameState } from "../classes/Game.js";
import { ServerMsgType } from "../classes/MessageType.js";
import { Player } from "../classes/Player.js";
import { WebSocket } from "ws";

export class PlayerUtils {
  /**
   * Called when ClientMsgType.UpdatePosition is received. Updates new position
   * for client in players state. Once it has received an update from all
   * different players, it will broadcast ServerMsgType.UpdatePlayersPosition
   * so that clients can update their local state.
   *
   * @param socketId the socketId of the player to update
   * @param data the new position of the player
   */
  public static HandleUpdatePlayerPosition(
    game: GameState,
    socketId: string,
    data: Position
  ) {
    game.players[socketId].UpdatePosition(data);

    // Update only once every player's position is updated
    game.updatedPositions.set(socketId, data);

    let updateList = [];
    for (const [key, value] of game.updatedPositions) {
      updateList.push({ socketId: key, position: value });
    }

    game.Broadcast({
      type: ServerMsgType.UpdatePlayersPosition,
      data: updateList,
    });
    game.updatedPositions = new Map<string, Position>();
  }

  // TODO COMMENT
  public static HandlePlayerEatenFood(
    gameState: GameState,
    socketId: string,
    msgData: string
  ): void {
    // 1. Parse msg data
    let blobId = msgData;

    // 2. Verify if player really ate food (TODO LATER)

    // 3. Update player's score
    let foodManager: FoodManager = gameState.foodManager;
    let foodBlob = foodManager.GetFoodBlobById(blobId);

    // 4. Update player's blob size
    gameState.players[socketId].blob.AddSize(foodBlob.size);
    foodManager.RemoveFoodBlobById(blobId);

    // 5. Broadcast PlayerAteFood to all players
    gameState.Broadcast({
      type: ServerMsgType.PlayerAteFood,
      data: {
        playerId: socketId,
        foodId: blobId,
      },
    });
  }

  // TODO COMMENT
  public static HandlePlayerEatenEnemy(
    game: GameState,
    socketId: string,
    otherPlayer: Player
  ) {
    const otherSocketId = otherPlayer.socketId;

    // Update player's size
    game.players[socketId].blob.AddSize(otherPlayer.blob.size);

    // Send message to all players, even the eaten player
    // This is because the close signal wates for the readyState to be 1
    game.Broadcast({
      type: ServerMsgType.PlayerAteEnemy,
      data: {
        playerWhoAte: socketId,
        newSize: game.players[socketId].blob.size,
        playerEaten: otherSocketId,
      },
    });

    // Remove eaten player from game
    game.players[otherSocketId].EatenByEnemy();
    let otherPlayerSocket: WebSocket = game.players[otherSocketId].socket;
    if (otherPlayerSocket && otherPlayerSocket.readyState === 1)
      game.players[otherSocketId].socket.close();

    delete game.players[otherSocketId];
    game.numPlayers--;

    console.log("Sending player ate enemy message!");
  }
}
