import { Position, Blob } from './Blob.js';
import * as uuid from "uuid";


/**
 * The mass of the food does follow the same rules as the mass of the blobs
 * of the players. 
 */
export class Food extends Blob {
    color: number;

    constructor(
        position: Position, 
        size: number
    ) {
        super(
            uuid.v4(), 
            position, 
            size + Math.floor(Math.random() * 2)
        );
        this.color = Math.floor(Math.random() * 0xFFFFFF);
    }   
}


export class FoodManager {
    food: Food[];
    defaultMassSize: number;
    mapSize: Position;
    maxFood: number;

    constructor(
        defaultMassSize: number,
        maxFood: number,
        mapSize: Position,
    ) {
        this.food = [];
        this.defaultMassSize = defaultMassSize;
        this.maxFood = maxFood;
        this.mapSize = mapSize;
    }


    AddFood() {
        if (this.food.length >= this.maxFood) return;

        // Uniform distribution of food across the map
        let x = Math.floor(Math.random() * this.mapSize.x);
        let y = Math.floor(Math.random() * this.mapSize.y);

        this.food.push(new Food({ x, y }, this.defaultMassSize));
    }


    RemoveFood(id: string) {
        this.food = this.food.filter((food) => food.id !== id);
    }
}