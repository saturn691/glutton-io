// Function to generate a normally distributed random number using Box-Muller transform
function gaussianRandom() {
    let u = 0, v = 0;
    while (u === 0) u = Math.random(); // Converting [0,1) to (0,1)
    while (v === 0) v = Math.random();
    return Math.sqrt(-2.0 * Math.log(u)) * Math.cos(2.0 * Math.PI * v);
}

function randomStep(stepSize) {
    let xStep = gaussianRandom() * stepSize;
    let yStep = gaussianRandom() * stepSize;
    return { x: xStep, y: yStep };
}

// Function to simulate the player's movement using Brownian motion
function simulateBrownianMotion(numSteps, stepSize, minX, maxX, minY, maxY) {
    let positions = []; // Array to store positions at each step
    let position = { current_x: 300, current_y: 300 }; // Starting position at the origin

    positions.push({ ...position }); // Add initial position to the positions array

    for (let i = 0; i < numSteps; i++) {
        const step = randomStep(stepSize);

        position.current_x += step.x;
        position.current_x = (position.current_x > maxX) ? maxX : position.current_x;
        position.current_x = (position.current_x < minX) ? minX : position.current_x;

        position.current_y += step.y;
        position.current_y = (position.current_y > maxX) ? maxY : position.current_y;
        position.current_y = (position.current_y < minX) ? minY : position.current_y;

        // Add new position to the positions array
        positions.push({ current_x: position.current_x, current_y: position.current_y });
    }

    return positions;
}

// Plotting the motion on canvas
function plotPositions(positions) {
    const canvas = document.getElementById('simulationCanvas');
    if (canvas.getContext) {
        const ctx = canvas.getContext('2d');

        ctx.beginPath();
        ctx.moveTo(positions[0].current_x, positions[0].current_y);

        positions.forEach(pos => {
            ctx.lineTo(pos.current_x, pos.current_y);
            ctx.moveTo(pos.current_x, pos.current_y);
        });

        ctx.strokeStyle = '#FF0000'; // Red line
        ctx.stroke();
    }
}

// Run the simulation and plot when the page loads
document.addEventListener('DOMContentLoaded', () => {
    let positions = simulateBrownianMotion(1000, 1);
    plotPositions(positions);
});

// Run the simulation and plot
let positions = simulateBrownianMotion(1000, 10, 0, 600, 0, 600); // Example usage
plotPositions(positions);