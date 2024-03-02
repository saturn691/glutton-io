import { Entity } from './Entity.js';

// Blob class extends Entity, with x and y as center coordinates and size as radius
export class Virus extends Entity {

    constructor(id, x, y, size, color) {
        super(id, x, y, size, color);
    }

    splitBlob(blob, player) {
        const minimumSizeToSplit = 10; // Example minimum size
        const newSize = blob.size / 2;
        if (newSize >= minimumSizeToSplit) {
            // Assuming horizontal displacement for simplicity
            const displacement = newSize; // The distance each new blob is from the original blob's center
            const newBlob1X = blob.x - displacement; // NewBlob1 is to the left
            const newBlob2X = blob.x + displacement; // NewBlob2 is to the right

            // Use the existing `addBlob` method to add the new blobs to the player
            player.addBlob(blob.id + "_1", newBlob1X, blob.y, newSize);
            player.addBlob(blob.id + "_2", newBlob2X, blob.y, newSize);

            // Mark the original blob as not alive or remove it
            blob.isAlive = false;
        } else {
            console.log("Blob is too small to split");
        }
    }


    collidesWith(blob) {
        const distance = Math.sqrt((this.x - blob.x) ** 2 + (this.y - blob.y) ** 2);
        return distance < this.size || distance < blob.size;
    }

    render(context) {
        super.render(context);      // Call the render method of the base class
    }
}
