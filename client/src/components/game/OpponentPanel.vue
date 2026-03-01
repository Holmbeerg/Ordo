<script setup lang="ts">
import { useGame } from '@/composables/useGame.ts';
const { opponentScore, opponentName, opponentIsConnected, opponentRackCount } = useGame();
</script>

<template>
    <div
        class="flex flex-col justify-center items-center h-full w-full gap-4 select-none text-center"
    >
        <div class="flex flex-col items-center gap-2">
            <span
                :class="[
                    'w-3 h-3 rounded-full transition-colors duration-500',
                    opponentIsConnected ? 'bg-green-400' : 'bg-gray-600',
                ]"
                :title="opponentIsConnected ? 'Connected' : 'Disconnected'"
            />
            <span class="text-sm font-semibold text-gray-200 break-words w-full leading-snug">
                {{ opponentName ?? 'Opponent' }}
            </span>
        </div>

        <div class="flex flex-col items-center gap-0.5">
            <span class="text-4xl font-bold text-white tabular-nums leading-none">{{
                opponentScore
            }}</span>
            <span class="text-xs text-gray-500 uppercase tracking-widest">points</span>
        </div>

        <div class="flex flex-col items-center gap-0.5">
            <span class="text-2xl font-semibold text-gray-300 tabular-nums leading-none">{{
                opponentRackCount
            }}</span>
            <span class="text-xs text-gray-500 uppercase tracking-widest">tiles on rack</span>
        </div>

        <div class="flex flex-col items-center gap-0.5">
            <span class="text-2xl font-mono text-gray-600 leading-none">—</span>
            <span class="text-xs text-gray-500 uppercase tracking-widest">time</span>
        </div>
    </div>
</template>

<style scoped></style>
