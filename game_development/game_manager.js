import { SpatialGrid } from './spatial_grid.js';
import { Blob } from './blob.js';
import { Food } from './food.js';
import { Virus } from './virus.js';
import { Entity } from './entity.js';
import { Player } from './player.js';


class GameStateManager {
    constructor(gridWidth, gridHeight, cellSize) {
        this.spatialGrid = new SpatialGrid(gridWidth, gridHeight, cellSize);
        this.players = []; // Assuming each player controls one or more blobs
        this.foods = []; // Array of Food objects
        this.viruses = []; // Array of Virus objects
    }

    addPlayer(player) {
        this.players.push(player);
    }

    addFood(food) {
        this.foods.push(food);
        // this.spatialGrid.insert(food); // We might want this here if we will not be updating food and viruses in update()
    }

    addVirus(virus) {
        this.viruses.push(virus);
        // this.spatialGrid.insert(virus);
    }

    updateEntities() {
        // Assuming entities have a method to update their positions
        this.players.forEach(player => {
            player.blobs.forEach(blob => {
                blob.update();
                if (blob.isAlive) {
                    this.spatialGrid.insert(blob);
                }
            });
        });

        // Foods and viruses might not move, but if they do, handle their updates similarly
    }

    update() {
        this.spatialGrid.clear(); // Prepare the spatial grid for the new frame

        // Re-populate the spatial grid
        this.foods.forEach(food => this.spatialGrid.insert(food));
        this.viruses.forEach(virus => this.spatialGrid.insert(virus));
        this.updateEntities(); // Update and re-insert player blobs

        // Now handle interactions
        this.handleInteractions();
    }

    handleInteractions() {
        // Iterate over each cell in the spatial grid
        for (let i = 0; i < this.spatialGrid.columns; i++) {
            for (let j = 0; j < this.spatialGrid.rows; j++) {
                const cell = this.spatialGrid.cells[i][j];

                // Blob-food interactions
                cell.blobs.forEach(blob => {
                    cell.food.forEach((food, foodIndex) => {
                        if (blob.collidesWith(food)) {
                            blob.eat(food);
                            // Remove food from global list to prevent re-insertion next frame
                            const globalFoodIndex = this.foods.indexOf(food);
                            if (globalFoodIndex > -1) {
                                this.foods.splice(globalFoodIndex, 1);
                            }
                            // TODO: respawn food periodically
                        }
                    });
                });

                // Blob-virus interactions, similar to blob-food interactions
                cell.blobs.forEach(blob => {
                    cell.viruses.forEach(virus => {
                        if (blob.collidesWith(virus)) {
                            virus.splitBlob(blob);
                        }
                    });
                });

                // Blob-Blob interactions
                for (let b1 = 0; b1 < cell.blobs.length; b1++) {
                    for (let b2 = b1 + 1; b2 < cell.blobs.length; b2++) {
                        const blob1 = cell.blobs[b1];
                        const blob2 = cell.blobs[b2];

                        if (blob1.collidesWith(blob2)) {
                            // Determine which blob is larger
                            if (blob1.size > blob2.size) {
                                blob1.eat(blob2);
                            } else if (blob2.size > blob1.size) {
                                blob2.eat(blob1);
                            }
                            // If sizes are equal, no consumption logic applies -> no action needed
                        }
                    }
                }
            }
        }
    }
}


const canvas = document.getElementById('gameCanvas');
const context = canvas.getContext('2d');

const gridWidth = 800;
const gridHeight = 600;
const cellSize = 100; // Example size, adjust based on your needs
const gameStateManager = new GameStateManager(gridWidth, gridHeight, cellSize);

// Example setup
const player = new Player('player1', '#00F'); // Example player
gameStateManager.addPlayer(player);
player.addBlob('blob1', 400, 300, 10); // Add a blob to the player

// Add some food and viruses
gameStateManager.addFood(new Food('food1', 100, 100, 5, '#0F0'));
gameStateManager.addVirus(new Virus('virus1', 300, 300, 15, '#F00'));

function gameLoop() {
    context.clearRect(0, 0, canvas.width, canvas.height); // Clear the canvas
    gameStateManager.spatialGrid.renderGrid(context); // Render the spatial grid

    // Update and render entities
    gameStateManager.update();
    gameStateManager.players.forEach(player => player.renderBlobs(context));
    gameStateManager.foods.forEach(food => food.render(context));
    gameStateManager.viruses.forEach(virus => virus.render(context));

    requestAnimationFrame(gameLoop);
}

// Run the simulation and plot when the page loads
document.addEventListener('DOMContentLoaded', () => {
    gameStateManager.spatialGrid.renderGrid(context); // Render the spatial grid
});



gameLoop(); // Start the game loop

