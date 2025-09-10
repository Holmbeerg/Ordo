export interface Tile {
    id: number;
    letter: string;
    value: number;
}

export interface Player {
    id: string;
    name: string;
    score: number;
    tiles: Tile[];
}

export interface GameState {
    id: string;
    board: (Tile | null)[][];
    tileBagCount: number;
    currentTurnPlayerId: string;
    isGameOver: boolean;
}
