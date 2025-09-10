<script setup lang="ts">
import TileRack from "@/components/game/TileRack.vue";
import PlayerActions from "@/components/game/PlayerActions.vue";
import GameBoard from "@/components/game/GameBoard.vue";
</script>

<template>
  <div class="flex flex-col h-screen">
    <!-- Player/Game Info Section -->
    <div class="flex items-center justify-center bg-gray-100 p-4 shadow">
      <div>Player Info / Time Left / Score</div>
    </div>

    <!-- Game Board Section -->
    <main class="flex-grow flex items-center justify-center bg-gray-50">
      <GameBoard />
    </main>

    <!-- Tile Rack and Player Actions Section -->
    <div class="flex flex-col items-center bg-gray-100 p-4">
      <TileRack />
      <PlayerActions />
    </div>
  </div>
</template>

<style scoped>
</style>