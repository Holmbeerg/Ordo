<script setup lang="ts">
import AppFooter from "@/components/AppFooter.vue";
import AppHeader from "@/components/AppHeader.vue";
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