import { computed } from 'vue';
import { storeToRefs } from 'pinia';
import { useGameStore } from '@/stores/gameStore.ts';
import { connectionState, submitWord as hubSubmitWord, passTurn as hubPassTurn, exchangeTiles as hubExchangeTiles } from '@/services/signalr.ts';
import type { Tile } from '@/types/game.ts';

export function useGame() {
    const gameStore = useGameStore();

    const {
        gameState,
        pendingPlacements,
        error,
        isMyTurn,
        isGameOver,
        board,
        myRack,
        myScore,
        opponentScore,
        opponentRackCount,
        tilesRemaining,
        hasPendingPlacements,
    } = storeToRefs(gameStore);

    const isConnecting = computed(() => connectionState.value === 'connecting');
    const isConnected = computed(() => connectionState.value === 'connected');

    async function submitWord() {
        if (!gameState.value || !hasPendingPlacements.value) return;

        try {
            await hubSubmitWord(gameState.value.gameId, pendingPlacements.value);
        } catch (e) {
            error.value = e instanceof Error ? e.message : 'Failed to submit word';
        }
    }

    async function passTurn() {
        if (!gameState.value) return;

        try {
            await hubPassTurn(gameState.value.gameId);
        } catch (e) {
            error.value = e instanceof Error ? e.message : 'Failed to pass turn';
        }
    }

    async function exchangeTiles(tiles: Tile[]) {
        if (!gameState.value || tiles.length === 0) return;

        try {
            await hubExchangeTiles(gameState.value.gameId, tiles);
        } catch (e) {
            error.value = e instanceof Error ? e.message : 'Failed to exchange tiles';
        }
    }

    return {
        // Connection state
        isConnecting,
        isConnected,
        // Game state (from store)
        gameState,
        pendingPlacements,
        error,
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
        placeTile: gameStore.placeTile,
        recallTiles: gameStore.recallTiles,
        clearError: gameStore.clearError,
        submitWord,
        passTurn,
        exchangeTiles,
    };
}
