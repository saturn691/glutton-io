import { Player } from './Player.js';
import { PlayerUtils } from '../utils/PlayerUtils.js';
import { GameState } from './Game.js';
import { Blob, Position } from './Blob.js';
import { WebSocket } from 'ws';


// Must agree with the client's number in Blob.cs
const BaseSpeed = 5


/**
 * Same a Player, but with a method to update the position randomly
 */
export class Bot extends Player {
  constructor(
    color: number,
    socket: WebSocket,
    socketId: string,
    position: Position,
    size: number,
    blobId: string
  ) {
    super(color, socket, socketId, position, size, blobId);
  }

  /** 
   * Uses the Box-Muller transform to generate a random number
   * @returns a random number from the standard normal distribution
   */
  private GaussianRandom() {
    let u = 0, v = 0;

    // Converting [0,1) to (0,1)
    while (u === 0) u = Math.random(); 
    while (v === 0) v = Math.random();
    
    return Math.sqrt(-2.0 * Math.log(u)) * Math.cos(2.0 * Math.PI * v);
  }

  /**
   * Must agree with the client's speed calculation
   * Otherwise the movement of the player will be incorrect 
   */
  private GetSpeed()
  {
    return BaseSpeed / Math.log(this.size);
  }


  /**
   * Collisions are handled client side so simulate the same effect here.
   */
  CheckCollisions(game: GameState)
  {
    for (const [socketId, player] of Object.entries(game.players)) {
      if (socketId === this.socketId) continue;
      if (Blob.WhoAteWho(this.blob, player.blob) === 1) {
        console.log("Bot has eaten player:")
        console.log(player.ToJson())
        PlayerUtils.HandlePlayerEatenEnemy(game, this.socketId, player);
      }
    }
  }


  /**
   * Moves the player's position by a random step
   * @param deltaTime the time between updates
   */
  Update(deltaTime: number) {
    // Generate random steps
    let xStep = this.GaussianRandom() * this.GetSpeed() * deltaTime;
    let yStep = this.GaussianRandom() * this.GetSpeed() * deltaTime;

    // Update the player's position
    let prevPosition = this.blob.position;
    this.blob.position = {
      x: prevPosition.x + xStep,
      y: prevPosition.y + yStep
    };
  }
}
