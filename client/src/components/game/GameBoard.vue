<script setup lang="ts">
import { inject, ref, onMounted, onUnmounted } from 'vue';
import { useGame } from '@/composables/useGame.ts';
import { SELECTED_TILE_KEY } from '@/composables/useSelectedTile.ts';
import BlankTilePicker from '@/components/game/BlankTilePicker.vue';
import type { Square, TilePlacement } from '@/types/game.ts';

const { board, isMyTurn, pendingPlacements, placeTile, gameState } = useGame();
const selectedTile = inject(SELECTED_TILE_KEY)!;

// Blank tile picker state
const showBlankPicker = ref(false);
const pendingBlankPlacement = ref<TilePlacement | null>(null);

function onBlankLetterPicked(letter: string) {
    if (!pendingBlankPlacement.value) return;
    pendingBlankPlacement.value.tile.letter = letter;
    placeTile(pendingBlankPlacement.value);
    showBlankPicker.value = false;
    pendingBlankPlacement.value = null;
}

function onBlankPickerCancel() {
    if (pendingBlankPlacement.value) {
        // Return the tile to the rack since it was already removed
        if (gameState.value) {
            gameState.value.myRack.push(pendingBlankPlacement.value.tile);
        }
    }
    showBlankPicker.value = false;
    pendingBlankPlacement.value = null;
}

const containerRef = ref<HTMLElement | null>(null);
const boardPx = ref(0);

let ro: ResizeObserver | null = null;
onMounted(() => {
    ro = new ResizeObserver(([entry]) => {
        const { width, height } = entry.contentRect;
        boardPx.value = Math.floor(Math.min(width, height));
    });
    if (containerRef.value) ro.observe(containerRef.value);
});
onUnmounted(() => ro?.disconnect());

function isCenter(r: number, c: number) {
    return r === 7 && c === 7;
}

function isPending(r: number, c: number) {
    return pendingPlacements.value.some((p) => p.row === r && p.col === c);
}

// We still need this for drag-and-drop events since they only pass row/col coordinates, not the square object
function getSquare(r: number, c: number): Square | null {
    return board.value?.[r]?.[c] ?? null;
}

function squareClass(sq: Square, r: number, c: number): string {
    if (!sq) return 'bg-[#4a7c59] border border-[#3d6b4a]';
    if (sq.tile)
        return isPending(r, c)
            ? 'bg-amber-300 border border-amber-500'
            : 'bg-amber-100 border border-amber-400';
    if (isCenter(r, c)) return 'bg-rose-500 border border-rose-700';
    switch (sq.multiplier) {
        case 'TripleWord':
            return 'bg-red-700 border border-red-900';
        case 'DoubleWord':
            return 'bg-rose-400 border border-rose-600';
        case 'TripleLetter':
            return 'bg-blue-600 border border-blue-800';
        case 'DoubleLetter':
            return 'bg-sky-400 border border-sky-600';
        default:
            return 'bg-[#4a7c59] border border-[#3d6b4a]';
    }
}

function squareLabel(sq: Square, r: number, c: number): string {
    if (!sq || sq.tile) return '';
    if (isCenter(r, c)) return '★';
    switch (sq.multiplier) {
        case 'TripleWord':
            return 'TW';
        case 'DoubleWord':
            return 'DW';
        case 'TripleLetter':
            return 'TL';
        case 'DoubleLetter':
            return 'DL';
        default:
            return '';
    }
}

function isClickable(sq: Square) {
    return isMyTurn.value && !!selectedTile.value && !sq.tile;
}

function onSquareClick(r: number, c: number) {
    if (isPending(r, c)) {
        recallSingle(r, c);
        return;
    }
    const sq = getSquare(r, c);
    if (!sq || !isClickable(sq)) return;

    const tile = selectedTile.value!;
    selectedTile.value = null;

    if (tile.isBlank) {
        // Remove tile from rack immediately so UI stays consistent
        if (gameState.value) {
            gameState.value.myRack = gameState.value.myRack.filter((t) => t.id !== tile.id);
        }
        pendingBlankPlacement.value = { row: r, col: c, tile };
        showBlankPicker.value = true;
    } else {
        placeTile({ row: r, col: c, tile });
    }
}

function recallSingle(r: number, c: number) {
    const sq = getSquare(r, c);
    if (!sq?.tile) return;
    const tile = sq.tile;
    sq.tile = null;
    const idx = pendingPlacements.value.findIndex((p) => p.row === r && p.col === c);
    if (idx !== -1) pendingPlacements.value.splice(idx, 1);
    if (tile.isBlank) tile.letter = '';
    if (gameState.value) gameState.value.myRack.push(tile);
    // Clear selection if this tile was selected
    if (selectedTile.value?.id === tile.id) selectedTile.value = null;
}

// Drag and drop, https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API
// https://www.w3schools.com/html/html5_draganddrop.asp
const dragOverSquare = ref<string | null>(null); // for highlighting
const draggingFromSquare = ref<string | null>(null); // where we started
const dropSucceeded = ref(false);

function canDrop(r: number, c: number): boolean {
    const sq = getSquare(r, c);
    return (
        isMyTurn.value &&
        !!sq &&
        (!sq.tile || isPending(r, c)) &&
        draggingFromSquare.value !== `${r}-${c}`
    );
}

function onSquareDragStart(e: DragEvent, r: number, c: number) {
    if (!isPending(r, c)) {
        e.preventDefault();
        return;
    }
    const sq = getSquare(r, c);
    if (!sq?.tile) return;
    e.dataTransfer!.effectAllowed = 'move';
    e.dataTransfer!.setData('application/ordo-tile', JSON.stringify(sq.tile)); // attach tile data to the drag event, needed later in onDrop
    dropSucceeded.value = false;
    draggingFromSquare.value = `${r}-${c}`; // track where we started so canDrop and onDrop know the source square
}

function onSquareDragEnd() {
    draggingFromSquare.value = null;
    dragOverSquare.value = null;
    dropSucceeded.value = false;
}

function onDragOver(e: DragEvent, r: number, c: number) {
    if (!canDrop(r, c)) return;
    e.preventDefault(); // signal to the browser to allow dropping
    e.dataTransfer!.dropEffect = 'move';
    dragOverSquare.value = `${r}-${c}`;
}

function onDragLeave() {
    dragOverSquare.value = null;
}

function onDrop(e: DragEvent, r: number, c: number) {
    dragOverSquare.value = null;
    const json = e.dataTransfer?.getData('application/ordo-tile');
    if (!json) return;
    const tile = JSON.parse(json);

    // if dragging from another board square, remove it from the source first (no rack involved)
    if (draggingFromSquare.value) {
        const [fromR, fromC] = draggingFromSquare.value.split('-').map(Number);
        if (fromR === r && fromC === c) {
            dropSucceeded.value = true;
            return;
        }
        const srcSq = getSquare(fromR, fromC);
        if (srcSq) srcSq.tile = null;
        const idx = pendingPlacements.value.findIndex((p) => p.row === fromR && p.col === fromC);
        if (idx !== -1) pendingPlacements.value.splice(idx, 1);
    }

    // If target has a pending tile, swap it back to rack
    if (isPending(r, c)) recallSingle(r, c);

    dropSucceeded.value = true;

    if (tile.isBlank) {
        // Always prompt for a letter when placing a blank tile (rack or board-to-board move)
        tile.letter = '';
        // If coming from the rack the tile hasn't been removed yet — do it now
        if (!draggingFromSquare.value && gameState.value) {
            gameState.value.myRack = gameState.value.myRack.filter((t) => t.id !== tile.id);
        }
        pendingBlankPlacement.value = { row: r, col: c, tile };
        showBlankPicker.value = true;
    } else {
        placeTile({ row: r, col: c, tile });
    }
    if (selectedTile.value?.id === tile.id) selectedTile.value = null;
}
</script>

<template>
    <div ref="containerRef" class="w-full h-full flex items-center justify-center overflow-hidden">
        <div
            v-if="boardPx > 0 && board"
            class="grid border-2 border-[#1a1917] overflow-hidden shadow-2xl flex-shrink-0"
            :style="{
                gridTemplateColumns: 'repeat(15, 1fr)',
                gridTemplateRows: 'repeat(15, 1fr)',
                width: boardPx + 'px',
                height: boardPx + 'px',
            }"
        >
            <template v-for="(rowArr, r) in board" :key="'row-' + r">
                <div
                    v-for="(square, c) in rowArr"
                    :key="`${r}-${c}`"
                    :draggable="isPending(r, c) ? 'true' : 'false'"
                    :class="[
                        'relative flex items-center justify-center transition-all duration-100 select-none',
                        squareClass(square, r, c),
                        isPending(r, c)
                            ? 'cursor-grab active:cursor-grabbing'
                            : isClickable(square)
                              ? 'cursor-pointer hover:brightness-125 z-10'
                              : 'cursor-default',
                        dragOverSquare === `${r}-${c}` ? 'brightness-150 scale-[1.05] z-10' : '',
                        draggingFromSquare === `${r}-${c}` ? 'opacity-40' : '',
                    ]"
                    @click="onSquareClick(r, c)"
                    @dragstart="onSquareDragStart($event, r, c)"
                    @dragend="onSquareDragEnd"
                    @dragover="onDragOver($event, r, c)"
                    @dragleave="onDragLeave"
                    @drop="onDrop($event, r, c)"
                >
                    <template v-if="square && square.tile">
                        <div
                            class="relative flex items-center justify-center w-full h-full leading-none select-none"
                        >
                            <span
                                class="font-bold text-gray-900"
                                :style="{ fontSize: Math.max(8, boardPx / 26) + 'px' }"
                            >
                                {{ square.tile.letter }}
                            </span>
                            <span
                                v-if="square.tile.value > 0"
                                class="absolute top-0 right-0.5 text-gray-600 font-semibold leading-none"
                                :style="{ fontSize: Math.max(5, boardPx / 54) + 'px' }"
                            >
                                {{ square.tile.value }}
                            </span>
                        </div>
                        <div
                            v-if="isPending(r, c)"
                            class="absolute inset-0 bg-black/0 hover:bg-black/20 transition-colors duration-100"
                        />
                    </template>
                    <template v-else>
                        <span
                            class="font-bold text-white/90 drop-shadow text-center leading-tight pointer-events-none select-none"
                            :style="{ fontSize: Math.max(5, boardPx / 64) + 'px' }"
                            >{{ squareLabel(square, r, c) }}</span
                        >
                    </template>
                </div>
            </template>
        </div>
    </div>

    <BlankTilePicker
        :visible="showBlankPicker"
        @pick="onBlankLetterPicked"
        @cancel="onBlankPickerCancel"
    />
</template>

<style scoped></style>
