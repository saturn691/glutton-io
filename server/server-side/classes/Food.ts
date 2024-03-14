import { Position, Blob } from "./Blob.js";
import * as uuid from "uuid";

/**
 * The mass of the food does follow the same rules as the mass of the blobs
 * of the players.
 */
export class Food extends Blob {
  color: number;

  constructor(position: Position, size: number) {
    super(uuid.v4(), position, size + Math.floor(Math.random() * 2));
    this.color = Math.floor(Math.random() * 0xffffff);
  }
}

export class FoodManager {
  foodBlobs: Map<string, Blob>;
  defaultMassSize: number;
  mapSize: Position;
  maxFood: number;

  constructor(defaultMassSize: number, maxFood: number, mapSize: Position) {
    this.foodBlobs = new Map<string, Blob>();
    this.defaultMassSize = defaultMassSize;
    this.maxFood = maxFood;
    this.mapSize = mapSize;
  }

  /**
   * Adds existing food blob to food dict
   */
  AddFoodBlob(newFoodBlob: Blob) {
    this.foodBlobs.set(newFoodBlob.id, newFoodBlob);
  }

  /**
   * Gets food blob from foodBlobs dict by id
   */
  GetFoodBlobById(id: string) {
    return this.foodBlobs.get(id);
  }

  /**
   * Spawns a new food blob and adds it to the foodBlobs dict
   */
  InitFoodBlob() {
    if (this.foodBlobs.size >= this.maxFood) return;

    // Uniform distribution of food across the map
    let x = Math.floor(Math.random() * this.mapSize.x);
    let y = Math.floor(Math.random() * this.mapSize.y);

    let newFoodBlob = new Blob(uuid.v4(), { x, y }, this.defaultMassSize);
    this.foodBlobs.set(newFoodBlob.id, newFoodBlob);

    return newFoodBlob;
  }

  /**
   * Removes food blob from the foodBlobs dict by blob id
   */
  RemoveFoodBlobById(id: string) {
    this.foodBlobs.delete(id);
  }
}
