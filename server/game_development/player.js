import { Blob } from './blob.js';
import { Entity } from './entity.js';

export class Player {

    constructor(id, color) {
        this.id = id;
        this.blobs = [];
        this.color = color;
        this.gameOver = false;
    }

    addBlob(blobID, x, y, size) {
        const blob = new Blob(blobID, x, y, size, this.color);
        this.blobs.push(blob);
    }

    updateBlobPositions(positionUpdates) {
        // 'positionUpdates' is an array of objects with {id, x, y} for each blob
        positionUpdates.forEach((update) => {
            // Find the blob by its ID
            const blob = this.blobs.find(blob => blob.id === update.id);
            if (blob) {
                // Update the blob's position
                blob.x = update.x;
                blob.y = update.y;
            }
        });
    }

    // Remove blobs that are not alive
    removeDeadBlobs() {
        this.blobs = this.blobs.filter(blob => blob.isAlive);
    }

    checkGameOver() {
        if (this.blobs.length === 0 || this.blobs.every(blob => !blob.isAlive)) {
            this.gameOver = true;
        }
    }

    // Render all blobs associated with the player
    renderBlobs(context) {
        this.blobs.forEach(blob => {
            if (blob.isAlive) { // Only render if the blob is alive
                blob.render(context);
            }
        });
    }


}
