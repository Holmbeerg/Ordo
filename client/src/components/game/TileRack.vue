<script setup lang="ts">
import { inject } from 'vue';
import { useGame } from '@/composables/useGame.ts';
import { SELECTED_TILE_KEY } from '@/composables/useSelectedTile.ts';
import type { Tile } from '@/types/game.ts';

const { myRack, isMyTurn } = useGame();
const selectedTile = inject(SELECTED_TILE_KEY);

function selectTile(tile: Tile) {
    if (!isMyTurn.value || !selectedTile) return;
    selectedTile.value = selectedTile.value?.id === tile.id ? null : tile;
}

function isSelected(tile: Tile): boolean {
    return selectedTile?.value?.id === tile.id;
}

// Use dataTransfer to carry the tile JSON. completely independent of selectedTile
// so the two interaction modes don't interfere with each other.
function onDragStart(e: DragEvent, tile: Tile) {
    if (!isMyTurn.value) return;
    e.dataTransfer!.effectAllowed = 'move';
    e.dataTransfer!.setData('application/ordo-tile', JSON.stringify(tile));
}
</script>

<template>
    <div class="flex flex-row gap-2 p-3 bg-[#3d3b38] rounded-lg shadow-lg border border-[#1a1917]">
        <div
            v-for="tile in myRack"
            :key="tile.id"
            :draggable="isMyTurn ? 'true' : 'false'"
            :class="[
                'relative w-11 h-11 flex flex-col items-center justify-center rounded font-bold shadow transition-all duration-100 select-none',
                isMyTurn
                    ? 'cursor-grab active:cursor-grabbing hover:scale-110 hover:-translate-y-1'
                    : 'cursor-default opacity-60',
                isSelected(tile)
                    ? 'bg-amber-300 scale-110 -translate-y-2 ring-2 ring-amber-400 shadow-lg'
                    : 'bg-[#f5e6c8] text-gray-900',
            ]"
            @click="selectTile(tile)"
            @dragstart="onDragStart($event, tile)"
        >
            <span class="text-gray-900 leading-none" style="font-size: clamp(12px, 2.5vw, 20px)">
                {{ tile.letter }}
            </span>
            <span
                v-if="tile.value > 0"
                class="absolute top-0 right-0.5 text-gray-500 font-semibold leading-none"
                style="font-size: clamp(6px, 1vw, 10px)"
            >
                {{ tile.value }}
            </span>
        </div>

        <!-- Empty rack slots -->
        <div
            v-for="i in Math.max(0, 7 - myRack.length)"
            :key="`empty-${i}`"
            class="w-11 h-11 rounded border border-dashed border-gray-600 bg-[#302E2B] opacity-40"
        />
    </div>
</template>

<style scoped></style>
