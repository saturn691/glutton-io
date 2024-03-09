export type Position = {
    x: number;
    y: number;
};

export class Blob {
    id: string;
    position: Position;
    size: number;

    constructor(
        id: string,
        position: Position,
        size: number
    ) {
        this.id = id;
        this.position = position;
        this.size = size;
    }

    EatEnemy(appendedSize: number) {
        this.size += appendedSize;
    }
}