import { describe, expect, it } from 'vitest';
import { calculatePreviewScore } from '@/utils/scoring.ts';
import type { Board, MultiplierType, Tile, TilePlacement } from '@/types/game.ts';

function createTile(
    letter: string,
    value: number,
    isBlank = false,
    id = `${letter}-${value}-${isBlank}`
): Tile {
    return {
        id,
        letter,
        value,
        isBlank,
    };
}

function createBoard(): Board {
    return Array.from({ length: 15 }, () =>
        Array.from({ length: 15 }, () => ({
            multiplier: 'None' as MultiplierType,
            tile: null,
        }))
    );
}

function applyPlacements(board: Board, placements: TilePlacement[]) {
    for (const placement of placements) {
        board[placement.row][placement.col].tile = placement.tile;
    }
}

describe('calculatePreviewScore', () => {
    it('scores a main word using square multipliers on newly placed tiles', () => {
        const board = createBoard();
        board[7][7].multiplier = 'DoubleWord';

        const placements: TilePlacement[] = [
            { row: 7, col: 7, tile: createTile('C', 8, false, 'c') },
            { row: 7, col: 8, tile: createTile('A', 1, false, 'a') },
            { row: 7, col: 9, tile: createTile('T', 1, false, 't') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(20);
    });

    it('does not re-apply multipliers to existing tiles in the completed word', () => {
        const board = createBoard();
        board[7][7].multiplier = 'DoubleWord';
        board[7][9].multiplier = 'DoubleLetter';
        board[7][8].tile = createTile('A', 1, false, 'existing-a');

        const placements: TilePlacement[] = [
            { row: 7, col: 7, tile: createTile('C', 8, false, 'c') },
            { row: 7, col: 9, tile: createTile('T', 1, false, 't') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(22);
    });

    it('scores both words when a single tile creates a horizontal and vertical word', () => {
        const board = createBoard();
        board[7][8].multiplier = 'DoubleLetter';
        board[7][7].tile = createTile('C', 8, false, 'c-existing');
        board[7][9].tile = createTile('T', 1, false, 't-existing');
        board[6][8].tile = createTile('H', 3, false, 'h-existing');
        board[8][8].tile = createTile('I', 1, false, 'i-existing');

        const placements: TilePlacement[] = [
            { row: 7, col: 8, tile: createTile('A', 1, false, 'a') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(17);
    });

    it('keeps blank tiles worth zero even on multiplier squares', () => {
        const board = createBoard();
        board[7][8].multiplier = 'DoubleLetter';
        board[7][7].tile = createTile('C', 8, false, 'c-existing');

        const placements: TilePlacement[] = [
            { row: 7, col: 8, tile: createTile('A', 0, true, 'blank-a') },
            { row: 7, col: 9, tile: createTile('T', 1, false, 't') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(9);
    });

    it('adds the 50-point bonus when all seven tiles are placed', () => {
        const board = createBoard();

        const placements: TilePlacement[] = [
            { row: 7, col: 3, tile: createTile('A', 1, false, '1') },
            { row: 7, col: 4, tile: createTile('B', 1, false, '2') },
            { row: 7, col: 5, tile: createTile('C', 1, false, '3') },
            { row: 7, col: 6, tile: createTile('D', 1, false, '4') },
            { row: 7, col: 7, tile: createTile('E', 1, false, '5') },
            { row: 7, col: 8, tile: createTile('F', 1, false, '6') },
            { row: 7, col: 9, tile: createTile('G', 1, false, '7') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(57);
    });

    it('returns 0 for a first move that does not cover the center square', () => {
        const board = createBoard();

        const placements: TilePlacement[] = [
            { row: 7, col: 8, tile: createTile('A', 1, false, 'a') },
            { row: 7, col: 9, tile: createTile('T', 1, false, 't') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(0);
    });

    it('returns 0 for a move that is not connected to existing tiles', () => {
        const board = createBoard();
        board[7][7].tile = createTile('H', 3, false, 'existing-h');

        const placements: TilePlacement[] = [
            { row: 2, col: 2, tile: createTile('C', 8, false, 'c') },
            { row: 2, col: 3, tile: createTile('A', 1, false, 'a') },
            { row: 2, col: 4, tile: createTile('T', 1, false, 't') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(0);
    });

    it('returns 0 for placements that bend into multiple directions', () => {
        const board = createBoard();
        board[7][7].tile = createTile('H', 4, false, 'existing-h');

        const placements: TilePlacement[] = [
            { row: 7, col: 8, tile: createTile('A', 1, false, 'a') },
            { row: 8, col: 9, tile: createTile('T', 1, false, 't') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(0);
    });

    it('returns 0 for placements with an unbridged gap', () => {
        const board = createBoard();
        board[7][7].tile = createTile('H', 4, false, 'existing-h');

        const placements: TilePlacement[] = [
            { row: 7, col: 8, tile: createTile('A', 1, false, 'a') },
            { row: 7, col: 10, tile: createTile('T', 1, false, 't') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(0);
    });

    it('allows gaps that are bridged by existing board tiles', () => {
        const board = createBoard();
        board[7][7].tile = createTile('H', 3, false, 'existing-h');
        board[7][9].tile = createTile('E', 1, false, 'existing-e');

        const placements: TilePlacement[] = [
            { row: 7, col: 8, tile: createTile('A', 1, false, 'a') },
            { row: 7, col: 10, tile: createTile('T', 1, false, 't') },
        ];

        applyPlacements(board, placements);

        expect(calculatePreviewScore(board, placements)).toBe(6);
    });

    it('returns 0 if a placement is on top of an already occupied square', () => {
        const board = createBoard();
        board[7][7].tile = createTile('H', 3, false, 'existing-h');
        board[7][8].tile = createTile('O', 1, false, 'existing-o');

        const placements: TilePlacement[] = [
            { row: 7, col: 8, tile: createTile('A', 1, false, 'a') },
        ];

        expect(calculatePreviewScore(board, placements)).toBe(0);
    });
});
