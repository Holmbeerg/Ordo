<script setup lang="ts">
import { useGame } from '@/composables/useGame.ts';

const { isMyTurn, gameState } = useGame();
</script>

<template>
    <div class="flex items-center gap-2">
        <span
            :class="[
                'w-2.5 h-2.5 rounded-full flex-shrink-0',
                isMyTurn ? 'bg-green-400 animate-pulse' : 'bg-gray-500',
            ]"
        />
        <span :class="['text-sm font-medium', isMyTurn ? 'text-green-300' : 'text-gray-400']">
            {{ isMyTurn ? 'Your turn' : `${gameState?.opponentName ?? 'Opponent'}'s turn` }}
        </span>
    </div>
</template>

<style scoped></style>
