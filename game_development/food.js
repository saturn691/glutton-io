import { Entity } from './entity.js';

// Blob class extends Entity, with x and y as center coordinates and size as radius
export class Food extends Entity {

    constructor(id, x, y, size, color) {
        super(id, x, y, size, color);
        this.isAlive = true;
    }

    isEaten() {
        this.isAlive = false;
    }

    render(context) {
        if (!this.isAlive) return;  // Do not render if the blob is not alive
        super.render(context);      // Call the render method of the base class
    }
}