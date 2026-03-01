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
const showHeader = computed(() => !route.meta.hideHeader)

</script>

<template>
  <div class="bg-[#302E2B] h-screen flex flex-col overflow-hidden">
    <AppHeader v-if="showHeader" class="flex-shrink-0" />
    <div class="flex-1 min-h-0 flex flex-col overflow-hidden">
      <RouterView/>
    </div>
    <AppFooter v-if="showFooter" class="flex-shrink-0"/>
  </div>
</template>