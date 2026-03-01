import { ref, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import type { TimeControl } from '@/types/TimeControl.ts';
import * as signalR from '@/services/signalr.ts';
import type { GameStateDto } from '@/types/game.ts';
import { useGameStore } from '@/stores/gameStore.ts';

// Composable. Could have just had this in HomeView, but separation of concerns + easier to unit test
// Testing it would typically require mounting the component and becomes an integration test instead of a unit test
// Downside is abstraction

export function useMatchmaking() {
    const router = useRouter();

    const searching = ref(false);
    const currentMode = ref<TimeControl>('Blitz');
    const searchTimeSeconds = ref(0);

    let searchTimer: number | null = null;

    function onMatchFound(gameState: GameStateDto) {
        useGameStore().setGameState(gameState);
        stopTimer();
        searching.value = false;
        router.push(`/game/${gameState.gameId}`);
    }

    const connection = signalR.createConnection();
    connection.on('MatchFound', onMatchFound); // when server shouts MatchFound, execute the function

    function startTimer() {
        searchTimeSeconds.value = 0;
        searchTimer = window.setInterval(() => {
            searchTimeSeconds.value++;
        }, 1000);
    }

    function stopTimer() {
        if (searchTimer !== null) {
            clearInterval(searchTimer);
            searchTimer = null;
        }
    }

    async function startSearch(mode: TimeControl) {
        searching.value = true;
        currentMode.value = mode;
        startTimer();
        await signalR.joinMatchmaking(mode);
    }

    async function cancelSearch() {
        stopTimer();
        searching.value = false;
        searchTimeSeconds.value = 0;
        await signalR.leaveMatchmaking(currentMode.value);
    }

    onUnmounted(() => {
        connection.off('MatchFound', onMatchFound);
        if (searching.value) {
            cancelSearch();
        }
    });

    return {
        searching,
        currentMode,
        searchTimeSeconds,
        startSearch,
        cancelSearch,
    };
}
