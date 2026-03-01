<script setup lang="ts">
import { ref, watch } from 'vue';
import { useToast } from 'vue-toastification';
import { useGame } from '@/composables/useGame.ts';

const {
    isMyTurn,
    hasPendingPlacements,
    myRack,
    tilesRemaining,
    submitWord,
    passTurn,
    exchangeTiles,
    recallTiles,
    error,
    clearError,
} = useGame();

const toast = useToast();

watch(error, (msg) => {
    if (!msg) return;
    toast.error(msg, { timeout: 3500 });
    clearError();
});

const exchangeOpen = ref(false);
const selectedForExchange = ref<Set<string>>(new Set());

function toggleExchangeSelect(tileId: string) {
    const next = new Set(selectedForExchange.value);
    if (next.has(tileId)) {
        next.delete(tileId);
    } else {
        next.add(tileId);
    }
    selectedForExchange.value = next;
}

async function confirmExchange() {
    const tilesToSwap = myRack.value.filter((t) => selectedForExchange.value.has(t.id));
    if (tilesToSwap.length === 0) return;
    await exchangeTiles(tilesToSwap);
    closeExchange();
}

function closeExchange() {
    exchangeOpen.value = false;
    selectedForExchange.value = new Set();
}
</script>

<template>
    <!-- Action buttons -->
    <div class="flex flex-row gap-3 flex-wrap justify-center">
        <button
            :disabled="!isMyTurn || !hasPendingPlacements"
            class="px-5 py-2 rounded-lg bg-green-600 hover:bg-green-700 text-white text-sm font-semibold disabled:opacity-40 disabled:cursor-not-allowed transition cursor-pointer shadow"
            @click="submitWord"
        >
            Submit
        </button>

        <button
            :disabled="!hasPendingPlacements"
            class="px-5 py-2 rounded-lg bg-yellow-600 hover:bg-yellow-700 text-white text-sm font-semibold disabled:opacity-40 disabled:cursor-not-allowed transition cursor-pointer shadow"
            @click="recallTiles"
        >
            Recall
        </button>

        <button
            :disabled="!isMyTurn || hasPendingPlacements"
            class="px-5 py-2 rounded-lg bg-[#4a4846] hover:bg-[#5a5856] text-gray-200 text-sm font-semibold disabled:opacity-40 disabled:cursor-not-allowed transition cursor-pointer shadow"
            @click="passTurn"
        >
            Pass
        </button>

        <button
            :disabled="!isMyTurn || hasPendingPlacements || tilesRemaining < 7"
            :title="tilesRemaining < 7 ? 'Need at least 7 tiles in bag to exchange' : ''"
            class="px-5 py-2 rounded-lg bg-[#4a4846] hover:bg-[#5a5856] text-gray-200 text-sm font-semibold disabled:opacity-40 disabled:cursor-not-allowed transition cursor-pointer shadow"
            @click="exchangeOpen = true"
        >
            Exchange
        </button>
    </div>

    <!-- Exchange modal -->
    <Teleport to="body">
        <Transition name="modal">
            <div
                v-if="exchangeOpen"
                class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm"
                @click.self="closeExchange"
            >
                <div
                    class="bg-[#2a2826] border border-[#3d3b38] rounded-2xl shadow-2xl p-6 flex flex-col gap-5 w-[480px] max-w-[95vw]"
                >
                    <!-- Header -->
                    <div class="flex items-center justify-between">
                        <h2 class="text-white font-semibold text-base">Exchange tiles</h2>
                        <button
                            class="text-gray-400 hover:text-white text-xl leading-none cursor-pointer transition-colors"
                            @click="closeExchange"
                        >
                            ✕
                        </button>
                    </div>

                    <p class="text-gray-400 text-sm">
                        Click the tiles you want to swap back into the bag. Selected tiles are
                        highlighted.
                    </p>

                    <!-- Tile picker, 7 tiles in one row -->
                    <div class="flex flex-row gap-2 justify-center py-2">
                        <button
                            v-for="tile in myRack"
                            :key="tile.id"
                            :class="[
                                'w-11 h-11 flex flex-col items-center justify-center rounded font-bold shadow transition-all duration-100 select-none cursor-pointer flex-shrink-0',
                                selectedForExchange.has(tile.id)
                                    ? 'bg-amber-300 ring-2 ring-amber-400 scale-110 -translate-y-1 text-gray-900'
                                    : 'bg-[#f5e6c8] text-gray-900 hover:scale-105 hover:-translate-y-0.5',
                            ]"
                            @click="toggleExchangeSelect(tile.id)"
                        >
                            <span class="leading-none text-base">{{ tile.letter }}</span>
                            <span class="leading-none text-[9px] text-gray-500">{{
                                tile.value > 0 ? tile.value : ''
                            }}</span>
                        </button>
                    </div>

                    <!-- Footer -->
                    <div class="flex gap-3 justify-center pt-1">
                        <button
                            class="px-4 py-2 rounded-lg bg-[#3d3b38] hover:bg-[#4a4846] text-gray-300 text-sm font-medium transition cursor-pointer"
                            @click="closeExchange"
                        >
                            Cancel
                        </button>
                        <button
                            :disabled="selectedForExchange.size === 0"
                            class="px-4 py-2 rounded-lg bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold disabled:opacity-40 disabled:cursor-not-allowed transition cursor-pointer"
                            @click="confirmExchange"
                        >
                            Swap
                            {{
                                selectedForExchange.size > 0 ? `(${selectedForExchange.size})` : ''
                            }}
                        </button>
                    </div>
                </div>
            </div>
        </Transition>
    </Teleport>
</template>

<style>
.modal-enter-active,
.modal-leave-active {
    transition: opacity 0.2s ease;
}
.modal-enter-from,
.modal-leave-to {
    opacity: 0;
}
.modal-enter-active > div,
.modal-leave-active > div {
    transition: transform 0.2s ease;
}
.modal-enter-from > div,
.modal-leave-to > div {
    transform: scale(0.95) translateY(8px);
}
</style>
