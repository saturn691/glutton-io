import { FoodManager } from "../classes/Food.js";
import { GameState } from "../classes/Game.js";
import { ServerMsgType } from "../classes/MessageType.js";

export class PlayerUtils {
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
}
