<script setup lang="ts">
import { useGame } from '@/composables/useGame.ts';
const { myScore } = useGame();
</script>

<template>
    <div
        class="flex flex-col justify-center items-center h-full w-full gap-4 select-none text-center"
    >
        <div class="flex flex-col items-center gap-1">
            <span class="text-sm font-semibold text-gray-200 leading-snug">You</span>
        </div>

        <div class="flex flex-col items-center gap-0.5">
            <span class="text-4xl font-bold text-white tabular-nums leading-none">{{
                myScore
            }}</span>
            <span class="text-xs text-gray-500 uppercase tracking-widest">points</span>
        </div>

        <div class="flex flex-col items-center gap-0.5">
            <span class="text-2xl font-mono text-gray-600 leading-none">—</span>
            <span class="text-xs text-gray-500 uppercase tracking-widest">time</span>
        </div>
    </div>
</template>

<style scoped></style>
