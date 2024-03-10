import { Player, PlayerColor } from './Player.js';
import { Position } from './Blob.js';
import { WebSocket } from 'ws';


/**
 * Same a Player, but with a method to update the position randomly
 */
export class Bot extends Player {
  constructor(
    color: PlayerColor,
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
   * Moves the player's position by a random step
   * @param stepSize multiplier for the random step
   */
  Update(stepSize: number) {
    // Generate random steps
    let xStep = this.GaussianRandom() * stepSize;
    let yStep = this.GaussianRandom() * stepSize;

    // Update the player's position
    let prevPosition = this.blob.position;
    this.blob.position = {
      x: prevPosition.x + xStep,
      y: prevPosition.y + yStep
    };
  }
}
