import { Blob } from './blob.js';
import { Food } from './food.js';
import { Virus } from './virus.js';
import { Entity } from './entity.js';

export class SpatialGrid {
    constructor(width, height, cellSize) {
        this.cellSize = cellSize;
        this.columns = Math.ceil(width / cellSize);
        this.rows = Math.ceil(height / cellSize);
        // Initialize cells with separate lists for blobs, food, and viruses
        this.cells = Array(this.columns).fill().map(() =>
            Array(this.rows).fill().map(() => ({ blobs: [], food: [], viruses: [] }))
        );
    }

    clear() {
        for (let i = 0; i < this.columns; i++) {
            for (let j = 0; j < this.rows; j++) {
                // Clear each list within the cells
                this.cells[i][j] = { blobs: [], food: [], viruses: [] };
            }
        }
    }

    insert(entity) {
        const x = Math.floor(entity.x / this.cellSize);
        const y = Math.floor(entity.y / this.cellSize);
        if (x >= 0 && x < this.columns && y >= 0 && y < this.rows) {
            // Insert entity into the appropriate list based on its type
            if (entity instanceof Blob) {
                this.cells[x][y].blobs.push(entity);
            } else if (entity instanceof Food) {
                this.cells[x][y].food.push(entity);
            } else if (entity instanceof Virus) {
                this.cells[x][y].viruses.push(entity);
            }
        }
    }

    renderGrid(context) {
        for (let i = 0; i < this.columns; i++) {
            for (let j = 0; j < this.rows; j++) {
                const x = i * this.cellSize;
                const y = j * this.cellSize;
                context.strokeStyle = '#ccc'; // Light gray color for the grid lines
                context.beginPath();
                context.rect(x, y, this.cellSize, this.cellSize);
                context.stroke();
            }
        }
    }
}

