import sat from "sat";

export type Position = {
  x: number;
  y: number;
};

export class Blob {
  id: string;
  position: Position;
  size: number;

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
        y: this.position.y
      },
      size: this.size
    }
  }

  EatEnemy(appendedSize: number) {
    this.size += appendedSize;
  }

  public AddSize(sizeToAdd: number) {
    this.size += sizeToAdd;
  }

  /**
   * Radius must agree with the client-side implementation
   * @returns the blob as a circle
   */
  private ToCircle() {
    let radius = Math.sqrt(this.size / Math.PI);

    return new sat.Circle(
      new sat.Vector(this.position.x, this.position.y),
      radius
    );
  }

  /**
   * Tests if two blobs are colliding
   * @returns 0, 1 (blob1 ate blob2), 2 (blob2 ate blob1)
   */
  static WhoAteWho(blob1: Blob, blob2: Blob) {
    if (!blob1 || !blob2) return 0;

    let a = blob1.ToCircle();
    let b = blob2.ToCircle();

    let response = new sat.Response();
    let collided = sat.testCircleCircle(a, b, response);

    if (!collided) return 0;
    else if (response.bInA) return 1;
    else if (response.aInB) return 2;

    return 0;
  }
}
