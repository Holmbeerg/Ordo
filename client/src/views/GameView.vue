<script setup lang="ts">
import { ref, provide } from 'vue';
import { useRouter } from 'vue-router';
import GameBoard from '@/components/game/GameBoard.vue';
import PlayerActions from '@/components/game/PlayerActions.vue';
import TileRack from '@/components/game/TileRack.vue';
import OpponentPanel from '@/components/game/OpponentPanel.vue';
import PlayerPanel from '@/components/game/PlayerPanel.vue';
import GameMenu from '@/components/game/GameMenu.vue';
import GameOverModal from '@/components/game/GameOverModal.vue';
import { useGame } from '@/composables/useGame.ts';
import { SELECTED_TILE_KEY } from '@/composables/useSelectedTile.ts';
import type { Tile } from '@/types/game.ts';

const {
    isGameOver,
    tilesRemaining,
    iWon,
    gameOverReason,
    myScore,
    opponentScore,
    opponentName,
    winnerId,
} = useGame();

const router = useRouter();

const selectedTile = ref<Tile | null>(null);
provide(SELECTED_TILE_KEY, selectedTile);

function handleBackToLobby() {
    router.push('/');
}

const legend = [
    { label: 'Triple Word', cls: 'bg-red-700' },
    { label: 'Double Word', cls: 'bg-rose-400' },
    { label: 'Triple Letter', cls: 'bg-blue-600' },
    { label: 'Double Letter', cls: 'bg-sky-400' },
] as const;
</script>

<template>
    <div class="relative flex flex-col h-full overflow-hidden">
        <GameMenu />

        <!-- Main area: left panel | board | right panel -->
        <div class="flex-1 min-h-0 flex overflow-hidden">
            <div class="flex-shrink-0 w-44">
                <OpponentPanel />
            </div>

            <div class="flex-1 min-w-0 overflow-hidden">
                <GameBoard />
            </div>

            <div class="flex-shrink-0 w-44">
                <PlayerPanel />
            </div>
        </div>

        <!-- Bottom bar: rack + actions -->
        <div
            class="flex-shrink-0 flex flex-col items-center gap-2 bg-[#3d3b38] border-t border-[#1a1917] px-4 py-3"
        >
            <div class="flex items-center justify-center gap-4 w-full">
                <div class="flex flex-col items-center justify-center select-none w-12">
                    <span class="text-xl font-bold text-white tabular-nums leading-none">{{
                        tilesRemaining
                    }}</span>
                    <span
                        class="text-[10px] text-gray-500 uppercase tracking-widest mt-0.5 whitespace-nowrap"
                        >tiles left</span
                    >
                </div>
                <TileRack />
            </div>
            <PlayerActions />
            <div class="flex flex-wrap justify-center gap-x-3 gap-y-1">
                <span
                    v-for="item in legend"
                    :key="item.label"
                    class="flex items-center gap-1 text-gray-500 text-xs"
                >
                    <span
                        :class="[item.cls, 'w-2 h-2 rounded-sm inline-block flex-shrink-0']"
                    ></span
                    >{{ item.label }}
                </span>
            </div>
        </div>
    </div>

    <GameOverModal
        :visible="isGameOver"
        :reason="gameOverReason"
        :i-won="iWon"
        :my-score="myScore"
        :opponent-score="opponentScore"
        :opponent-name="opponentName"
        :winner-id="winnerId"
        @back-to-lobby="handleBackToLobby"
    />
</template>

<style>
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.3s ease;
}
.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>
