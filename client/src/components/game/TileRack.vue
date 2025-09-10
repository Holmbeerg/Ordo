<script setup lang="ts">
const tiles = ['A', 'B', 'C', 'D', 'E', 'F', 'G'];
</script>

<template>
  <div class="flex flex-row gap-2 p-2 bg-gray-200 rounded shadow">
    <div
        v-for="(tile, idx) in tiles"
        :key="idx"
        class="w-12 h-12 flex items-center justify-center bg-white border border-gray-300 rounded text-xl font-bold text-gray-800"
    >
      {{ tile }}
    </div>
  </div>
</template>

<style scoped>
</style>