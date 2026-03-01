<script setup lang="ts">
import { ref, provide } from 'vue';
import GameBoard from '@/components/game/GameBoard.vue';
import PlayerActions from '@/components/game/PlayerActions.vue';
import TileRack from '@/components/game/TileRack.vue';
import OpponentPanel from '@/components/game/OpponentPanel.vue';
import PlayerPanel from '@/components/game/PlayerPanel.vue';
import GameMenu from '@/components/game/GameMenu.vue';
import { useGame } from '@/composables/useGame.ts';
import { SELECTED_TILE_KEY } from '@/composables/useSelectedTile.ts';
import type { Tile } from '@/types/game.ts';

const { isGameOver, tilesRemaining } = useGame();

// Shared selected tile state. Provided to GameBoard and TileRack
const selectedTile = ref<Tile | null>(null);
provide(SELECTED_TILE_KEY, selectedTile);

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

        <!-- Game over banner -->
        <Transition name="fade">
            <div
                v-if="isGameOver"
                class="flex-shrink-0 text-center bg-amber-900/70 border-b border-amber-600 text-amber-200 px-5 py-2 font-semibold text-sm"
            >
                Game over!
            </div>
        </Transition>

        <!-- Main area: left panel | board | right panel -->
        <div class="flex-1 min-h-0 flex overflow-hidden">
            <div class="flex-shrink-0 w-44">
                <OpponentPanel />
            </div>

            <!-- Centre: board -->
            <div class="flex-1 min-w-0 overflow-hidden">
                <GameBoard />
            </div>

            <!-- Right: you -->
            <div class="flex-shrink-0 w-44">
                <PlayerPanel />
            </div>
        </div>

        <!-- Bottom bar: rack + actions -->
        <div
            class="flex-shrink-0 flex flex-col items-center gap-2 bg-[#3d3b38] border-t border-[#1a1917] px-4 py-3"
        >
            <!-- Tiles-left + rack centered together as one unit -->
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
            <!-- Actions centered independently -->
            <PlayerActions />
            <!-- Legend -->
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
