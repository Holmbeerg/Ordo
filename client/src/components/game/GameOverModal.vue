<script setup lang="ts">
import type { GameOverReason } from '@/types/game.ts';

const props = defineProps<{
    visible: boolean;
    reason: GameOverReason | null;
    iWon: boolean;
    myScore: number;
    opponentScore: number;
    opponentName: string | null;
    winnerId: string | null;
}>();

const emit = defineEmits<{
    backToLobby: [];
}>();

function headline(): string {
    if (props.reason === 'Resignation') return 'Opponent resigned. You win!';
    if (!props.winnerId) return "It's a draw!";
    if (props.iWon) return 'You win!';
    return 'You lose!';
}

function subline(): string {
    if (props.reason === 'Resignation') return 'Your opponent gave up.';
    if (!props.winnerId) return 'Both players finished with equal points.';
    if (props.reason === 'ConsecutivePasses')
        return props.iWon
            ? 'Game ended by passes. You won on points!'
            : 'Game ended by passes. You lost on points.';
    return 'Good game!';
}
</script>

<template>
    <Teleport to="body">
        <Transition name="modal">
            <div
                v-if="visible"
                class="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm"
            >
                <div
                    class="bg-[#2a2826] border border-[#3d3b38] rounded-2xl shadow-2xl p-8 flex flex-col gap-6 w-[380px] max-w-[95vw] items-center text-center"
                >
                    <!-- Icon + headline -->
                    <div class="flex flex-col items-center gap-2">
                        <span class="text-5xl" role="img" :aria-label="headline()">
                            {{ !winnerId ? '🤝' : iWon ? '🏆' : '😔' }}
                        </span>
                        <h2 class="text-white font-bold text-2xl leading-tight">
                            {{ headline() }}
                        </h2>
                        <p class="text-gray-400 text-sm">{{ subline() }}</p>
                    </div>

                    <!-- Scores -->
                    <div
                        class="w-full bg-[#1e1c1a] rounded-xl border border-[#3d3b38] divide-y divide-[#3d3b38]"
                    >
                        <div class="flex items-center justify-between px-5 py-3">
                            <span class="text-gray-300 text-sm font-medium">You</span>
                            <span
                                class="font-bold tabular-nums"
                                :class="iWon ? 'text-amber-300' : 'text-white'"
                            >
                                {{ myScore }}
                            </span>
                        </div>
                        <div class="flex items-center justify-between px-5 py-3">
                            <span class="text-gray-300 text-sm font-medium">
                                {{ opponentName ?? 'Opponent' }}
                            </span>
                            <span
                                class="font-bold tabular-nums"
                                :class="!iWon ? 'text-amber-300' : 'text-white'"
                            >
                                {{ opponentScore }}
                            </span>
                        </div>
                    </div>

                    <!-- Back to lobby -->
                    <button
                        class="w-full py-2.5 rounded-xl bg-amber-500 hover:bg-amber-400 active:scale-95 text-gray-900 font-semibold text-sm transition cursor-pointer shadow"
                        @click="emit('backToLobby')"
                    >
                        Back to lobby
                    </button>
                </div>
            </div>
        </Transition>
    </Teleport>
</template>

<style>
.modal-enter-active,
.modal-leave-active {
    transition:
        opacity 0.25s ease,
        transform 0.25s ease;
}
.modal-enter-from,
.modal-leave-to {
    opacity: 0;
    transform: scale(0.95);
}
</style>
