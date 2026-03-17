import type { Board, Tile, TilePlacement } from '@/types/game.ts';

interface FormedWord {
    text: string;
    placements: TilePlacement[];
}

const BOARD_SIZE = 15;

function createPlacementKey(row: number, col: number): string {
    return `${row}:${col}`;
}

function hasPlacementAt(placements: TilePlacement[], row: number, col: number): boolean {
    return placements.some((placement) => placement.row === row && placement.col === col);
}

function hasExistingTileAt(
    board: Board,
    placements: TilePlacement[],
    row: number,
    col: number
): boolean {
    if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) return false;
    if (hasPlacementAt(placements, row, col)) return false;

    return board[row]?.[col]?.tile !== null;
}

function isFirstTurn(board: Board, placements: TilePlacement[]): boolean {
    return !hasExistingTileAt(board, placements, 7, 7);
}

function isConnectedToExistingTiles(board: Board, placements: TilePlacement[]): boolean {
    return placements.some(
        (placement) =>
            hasExistingTileAt(board, placements, placement.row - 1, placement.col) ||
            hasExistingTileAt(board, placements, placement.row + 1, placement.col) ||
            hasExistingTileAt(board, placements, placement.row, placement.col - 1) ||
            hasExistingTileAt(board, placements, placement.row, placement.col + 1)
    );
}

function hasNoGaps(board: Board, placements: TilePlacement[], isHorizontal: boolean): boolean {
    if (isHorizontal) {
        const row = placements[0].row;
        const minCol = Math.min(...placements.map((placement) => placement.col));
        const maxCol = Math.max(...placements.map((placement) => placement.col));

        for (let col = minCol; col <= maxCol; col++) {
            const hasTileBeingPlaced = hasPlacementAt(placements, row, col);
            const hasExistingTile = hasExistingTileAt(board, placements, row, col);

            if (!hasTileBeingPlaced && !hasExistingTile) return false;
        }

        return true;
    }

    const col = placements[0].col;
    const minRow = Math.min(...placements.map((placement) => placement.row));
    const maxRow = Math.max(...placements.map((placement) => placement.row));

    for (let row = minRow; row <= maxRow; row++) {
        const hasTileBeingPlaced = hasPlacementAt(placements, row, col);
        const hasExistingTile = hasExistingTileAt(board, placements, row, col);

        if (!hasTileBeingPlaced && !hasExistingTile) return false;
    }

    return true;
}

function isValidPlacementForPreview(board: Board, placements: TilePlacement[]): boolean {
    if (placements.length === 0) return false;

    const placementKeys = new Set<string>();

    for (const placement of placements) {
        if (
            placement.row < 0 ||
            placement.row >= BOARD_SIZE ||
            placement.col < 0 ||
            placement.col >= BOARD_SIZE
        ) {
            return false;
        }

        const key = createPlacementKey(placement.row, placement.col);
        if (placementKeys.has(key)) return false;
        placementKeys.add(key);

        const boardTile = board[placement.row]?.[placement.col]?.tile;
        if (boardTile !== null && boardTile?.id !== placement.tile.id) {
            return false;
        }

        if (hasExistingTileAt(board, placements, placement.row, placement.col)) {
            return false;
        }
    }

    const isHorizontal = placements.every((placement) => placement.row === placements[0].row);
    const isVertical = placements.every((placement) => placement.col === placements[0].col);

    if (placements.length > 1 && !isHorizontal && !isVertical) return false;

    if (isFirstTurn(board, placements)) {
        return placements.some((placement) => placement.row === 7 && placement.col === 7);
    }

    if (!isConnectedToExistingTiles(board, placements)) return false;

    return !(placements.length > 1 && !hasNoGaps(board, placements, isHorizontal));
}

function getTileAt(
    board: Board,
    placements: TilePlacement[],
    row: number,
    col: number
): Tile | null {
    if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) return null;

    const placement = placements.find((p) => p.row === row && p.col === col);
    if (placement) return placement.tile;

    return board[row]?.[col]?.tile ?? null;
}

function getFullWord(
    board: Board,
    placements: TilePlacement[],
    row: number,
    col: number,
    isHorizontal: boolean
): FormedWord {
    const wordPlacements: TilePlacement[] = [];

    if (isHorizontal) {
        let startCol = col;
        while (getTileAt(board, placements, row, startCol - 1) !== null) {
            startCol--;
        }

        let currentCol = startCol;
        while (true) {
            const tile = getTileAt(board, placements, row, currentCol);
            if (tile === null) break;

            wordPlacements.push({ row, col: currentCol, tile });
            currentCol++;
        }
    } else {
        let startRow = row;
        while (getTileAt(board, placements, startRow - 1, col) !== null) {
            startRow--;
        }

        let currentRow = startRow;
        while (true) {
            const tile = getTileAt(board, placements, currentRow, col);
            if (tile === null) break;

            wordPlacements.push({ row: currentRow, col, tile });
            currentRow++;
        }
    }

    return {
        text: wordPlacements.map((placement) => placement.tile.letter).join(''),
        placements: wordPlacements,
    };
}

export function extractAllNewWords(board: Board, placements: TilePlacement[]): FormedWord[] {
    const newWords: FormedWord[] = [];
    if (placements.length === 0) return newWords;

    if (placements.length === 1) {
        const [placement] = placements;
        const horizontalWord = getFullWord(board, placements, placement.row, placement.col, true);
        const verticalWord = getFullWord(board, placements, placement.row, placement.col, false);

        if (horizontalWord.text.length > 1) newWords.push(horizontalWord);
        if (verticalWord.text.length > 1) newWords.push(verticalWord);

        return newWords;
    }

    const isMainHorizontal = placements[0].row === placements[1].row;

    const mainWord = getFullWord(
        board,
        placements,
        placements[0].row,
        placements[0].col,
        isMainHorizontal
    );
    if (mainWord.text.length > 1) newWords.push(mainWord);

    for (const placement of placements) {
        const crossWord = getFullWord(
            board,
            placements,
            placement.row,
            placement.col,
            !isMainHorizontal
        );

        if (crossWord.text.length > 1) newWords.push(crossWord);
    }

    return newWords;
}

export function calculatePreviewScore(board: Board, placements: TilePlacement[]): number {
    if (!isValidPlacementForPreview(board, placements)) return 0;

    const extractedWords = extractAllNewWords(board, placements);
    const placementKeys = new Set(
        placements.map((placement) => createPlacementKey(placement.row, placement.col))
    );

    let totalTurnScore = 0;

    for (const word of extractedWords) {
        let wordScore = 0;
        let wordMultiplier = 1;

        for (const wordPlacement of word.placements) {
            let letterScore = wordPlacement.tile.isBlank ? 0 : wordPlacement.tile.value;
            const isNewPlacement = placementKeys.has(`${wordPlacement.row}:${wordPlacement.col}`);

            if (isNewPlacement) {
                const square = board[wordPlacement.row]?.[wordPlacement.col];

                switch (square?.multiplier) {
                    case 'DoubleLetter':
                        letterScore *= 2;
                        break;
                    case 'TripleLetter':
                        letterScore *= 3;
                        break;
                    case 'DoubleWord':
                        wordMultiplier *= 2;
                        break;
                    case 'TripleWord':
                        wordMultiplier *= 3;
                        break;
                }
            }

            wordScore += letterScore;
        }

        totalTurnScore += wordScore * wordMultiplier;
    }

    if (placements.length === 7) {
        totalTurnScore += 50;
    }

    return totalTurnScore;
}
