import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { GameStateDto, TilePlacement } from '@/types/game.ts';

export const useGameStore = defineStore('game', () => {
    const gameState = ref<GameStateDto | null>(null);
    const pendingPlacements = ref<TilePlacement[]>([]);
    const error = ref<string | null>(null);

    const isMyTurn = computed(
        () => gameState.value?.currentTurnPlayerId === gameState.value?.myPlayerId
    );
    const isGameOver = computed(() => gameState.value?.status === 'Completed');
    const board = computed(() => gameState.value?.board ?? null);
    const myRack = computed(() => gameState.value?.myRack ?? []);
    const myScore = computed(() => gameState.value?.myScore ?? 0);
    const opponentScore = computed(() => gameState.value?.opponentScore ?? 0);
    const opponentRackCount = computed(() => gameState.value?.opponentRackCount ?? 0);
    const tilesRemaining = computed(() => gameState.value?.tilesRemainingInBag ?? 0);
    const hasPendingPlacements = computed(() => pendingPlacements.value.length > 0);

    function setGameState(dto: GameStateDto) {
        gameState.value = dto;
        pendingPlacements.value = []; // clear pending on every server update
    }

    function placeTile(placement: TilePlacement) {
        if (!gameState.value) return;

        // Remove tile from rack
        gameState.value.myRack = gameState.value.myRack.filter((t) => t.id !== placement.tile.id);

        // Write tile onto the board so board and rack stay in sync
        gameState.value.board.squares[placement.row][placement.col].tile = placement.tile;

        pendingPlacements.value.push(placement);
    }

    function recallTiles() {
        if (!gameState.value) return;

        // Remove tiles from the board for each pending placement
        for (const p of pendingPlacements.value) {
            gameState.value.board.squares[p.row][p.col].tile = null;
        }

        // Return tiles to rack
        gameState.value.myRack.push(...pendingPlacements.value.map((p) => p.tile));
        pendingPlacements.value = [];
    }

    function clearError() {
        error.value = null;
    }

    return {
        // State
        gameState,
        pendingPlacements,
        error,
        // Getters
        isMyTurn,
        isGameOver,
        board,
        myRack,
        myScore,
        opponentScore,
        opponentRackCount,
        tilesRemaining,
        hasPendingPlacements,
        // Actions
        setGameState,
        placeTile,
        recallTiles,
        clearError,
    };
});
