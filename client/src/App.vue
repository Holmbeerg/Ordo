<script setup lang="ts">
import {computed, onMounted, onUnmounted} from 'vue';
import AppFooter from "@/components/AppFooter.vue";
import AppHeader from "@/components/AppHeader.vue";
import { connectToHub, disconnectFromHub } from '@/services/signalr';
import {useRoute} from "vue-router";
const route = useRoute();

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

const showFooter = computed(() => !route.meta.hideFooter)

</script>

<template>
  <div class="bg-[#302E2B] min-h-screen">
    <AppHeader />
      <RouterView/>
    <AppFooter v-if="showFooter"/>
  </div>
</template>