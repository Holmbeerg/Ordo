<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import AppFooter from "@/components/AppFooter.vue";
import AppHeader from "@/components/AppHeader.vue";
import { connectToHub, disconnectFromHub } from '@/services/signalr';

onMounted(async () => {
  try {
    await connectToHub();
    console.log('App connected to SignalR hub');
  } catch (error) {
    console.error('App failed to connect to SignalR hub:', error);
  }
})

onUnmounted(async () => {
  await disconnectFromHub();
  console.log('App disconnected from SignalR hub');
})

</script>

<template>
  <div class="flex flex-col min-h-screen bg-[#302E2B]">
    <AppHeader/>
    <main class="container mx-auto p-4 flex-grow">
      <RouterView/>
    </main>
    <AppFooter/>
  </div>
</template>