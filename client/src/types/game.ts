export type MultiplierType = 'None' | 'DoubleLetter' | 'TripleLetter' | 'DoubleWord' | 'TripleWord';

export type GameStatus = 'WaitingForPlayers' | 'InProgress' | 'Completed';

export interface Tile {
    id: string;
    letter: string;
    value: number;
    isBlank: boolean;
}

export interface Square {
    multiplier: MultiplierType;
    tile: Tile | null;
}

export type Board = Square[][];

export interface TilePlacement {
    row: number;
    col: number;
    tile: Tile;
}

export interface GameStateDto {
    gameId: string;
    status: GameStatus;
    currentTurnPlayerId: string | null;
    board: Board;

    myPlayerId: string;
    myRack: Tile[];
    myScore: number;

    opponentId: string | null;
    opponentName: string | null;
    opponentScore: number;
    opponentRackCount: number;
    opponentIsConnected: boolean;

    tilesRemainingInBag: number;
}
