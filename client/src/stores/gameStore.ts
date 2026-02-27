import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { GameStateDto, TilePlacement } from "@/types/game.ts";

export const useGameStore = defineStore('game', () => {
    const gameState = ref<GameStateDto | null>(null);
    const pendingPlacements = ref<TilePlacement[]>([]);
    const isConnecting = ref(false);
    const error = ref<string | null>(null);

    const isMyTurn = computed(() =>
        gameState.value?.currentTurnPlayerId === gameState.value?.myPlayerId
    );
    const board = computed(() => gameState.value?.board ?? null);
    const myRack = computed(() => gameState.value?.myRack ?? []);
    const myScore = computed(() => gameState.value?.myScore ?? 0);
    const opponentScore = computed(() => gameState.value?.opponentScore ?? 0);
    const tilesRemaining = computed(() => gameState.value?.tilesRemainingInBag ?? 0);

    function setGameState(dto: GameStateDto) {
        gameState.value = dto;
    }

    function placeTile(placement: TilePlacement) {
        if (gameState.value) {
            // Remove from rack
            gameState.value.myRack = gameState.value.myRack.filter(
                (t) => t.id !== placement.tile.id
            );
        }
        pendingPlacements.value.push(placement);
    }

    function recallTiles() {
        if (!gameState.value) return;

        // Return pending tiles back to rack
        gameState.value.myRack.push(
            ...pendingPlacements.value.map((p) => p.tile) // spread operator
        );
        pendingPlacements.value = [];
    }

    function clearError() {
        error.value = null;
    }

    return {
        // State
        gameState,
        pendingPlacements,
        isConnecting,
        error,
        // Getters
        isMyTurn,
        board,
        myRack,
        myScore,
        opponentScore,
        tilesRemaining,
        // Actions
        setGameState,
        placeTile,
        recallTiles,
        clearError
    };
});