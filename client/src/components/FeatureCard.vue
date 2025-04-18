<script setup lang="ts">
defineProps({
  title: String,
  description: String
});
</script>

<template>
  <div class="bg-white p-6 rounded-lg shadow-md">
    <h3 class="text-xl font-bold mb-2">{{ title }}</h3>
    <p>{{ description }}</p>
  </div>
</template>