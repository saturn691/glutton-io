import { Entity } from './Entity.js';

// Blob class extends Entity, with x and y as center coordinates and size as radius
export class Blob extends Entity {

    constructor(id, x, y, size, color) {
        super(id, x, y, size, color);
        this.isAlive = true; // Indicates whether the blob is alive
    }

    move(xCoordinate, yCoordinate) {
        if (!this.isAlive) return;
        this.x = xCoordinate;
        this.y = yCoordinate;
    }

    eat(target) {
        // Check if the target is alive if it has the isAlive property; assume true if the property is absent.
        const targetIsAlive = target.isAlive !== undefined ? target.isAlive : true;

        if (!this.isAlive || !target.isAlive) return;

        const sizeDifferenceThreshold = 1.1; // TODO: modify threshold 
        const canEat = this.size > target.size && (this.size / target.size) > sizeDifferenceThreshold && this.collidesWith(target);

        if (canEat) {
            this.size += target.size; // Simulate the blob growing in size
            if (typeof target.isEaten === 'function') {
                target.isEaten();
            }
        }
    }

    isEaten() {
        this.isAlive = false;
    }

    collidesWith(otherEntity) {
        const distance = Math.sqrt((this.x - otherEntity.x) ** 2 + (this.y - otherEntity.y) ** 2);
        return distance < this.size || distance < otherEntity.size;
    }

    render(context) {
        if (!this.isAlive) return;  // Do not render if the blob is not alive
        super.render(context);      // Call the render method of the base class
    }
}
