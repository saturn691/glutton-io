// Official numbers used by the real game
const MassMultiplier = 1.1;
// const DistanceMultiplier = 1.6; // original
const DistanceMultiplier = 1.55;

export type Position = {
  x: number;
  y: number;
};

export class Blob {
  id: string;
  position: Position;
  size: number;
  static defaultSize = 1;

  constructor(id: string, position: Position, size: number) {
    this.id = id;
    this.position = position;
    this.size = size;
  }

  ToJson() {
    return {
      id: this.id,
      position: {
        x: this.position.x,
        y: this.position.y,
      },
      size: this.size,
    };
  }

  EatEnemy(appendedSize: number) {
    this.size += appendedSize;
  }

  public AddSize(sizeToAdd: number) {
    this.size += sizeToAdd;
  }

  /**
   * Tests if two blobs are colliding
   * @returns 0, 1 (blob1 ate blob2), 2 (blob2 ate blob1)
   */
  static WhoAteWho(blob1: Blob, blob2: Blob) {
    if (!blob1 || !blob2) return 0;

    if (blob1.BiggerThan(blob2) && blob1.Intersects(blob2)) {
      return 1;
    } else if (blob2.BiggerThan(blob1) && blob2.Intersects(blob1)) {
      return 2;
    }

    return 0;
  }

  /**
   * Checks if this blob is sufficiently bigger than the given blob.
   * Must agree with the client's calculation.
   */
  private BiggerThan(blob: Blob) {
    return this.size > blob.size * MassMultiplier;
  }

  /**
   * Check if this blob is sufficiently close to the given blob.
   * Must agree with the client's calculation.
   */
  private Intersects(blob: Blob) {
    let deltaX = this.position.x - blob.position.x;
    let deltaY = this.position.y - blob.position.y;

    let distanceBetweenCentres = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
    let thisRadius = Math.sqrt(this.size / Math.PI);

    return thisRadius > distanceBetweenCentres * DistanceMultiplier;
  }
}
