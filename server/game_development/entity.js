// Base class for all entities in the game. 
// The x and y coordinates represent the center of the entity, and size component is the radius.

export class Entity {
    constructor(id, x, y, radius, color) {
        this.id = id;
        this.x = x;
        this.y = y;
        this.radius = radius;
        this.color = color;
    }

    collidesWith(entity) {
        const distance = Math.sqrt((this.x - entity.x) ** 2 + (this.y - entity.y) ** 2);
        return distance < this.radius || distance < entity.radius;
    }

    // Basic render method - can be overridden or extended by subclasses
    render(context) {
        context.fillStyle = this.color;
        context.beginPath();
        context.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
        context.fill();
    }
}
