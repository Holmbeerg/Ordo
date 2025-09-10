https://vuejs.org/guide/essentials/class-and-style

<template>
  <nav class="container mx-auto flex justify-between items-center">
    <div class="flex gap-4 text-2xl font-bold">
      <router-link to="/" class="hover:underline"> Ordo </router-link>
    </div>

    <div class="flex items-center gap-2">
      <button
          @click="setLanguage('se')"
          class="p-1 rounded transition-transform hover:scale-110 cursor-pointer"
          :class="{ 'ring-2 ring-blue-400': currentLanguage === 'se' }"
      >
        <img src="@/assets/swedish-flag4x3.svg" alt="Swedish" class="w-8 h-6" />
      </button>

      <span class="text-gray-400">/</span>

      <button
          @click="setLanguage('en')"
          class="p-1 rounded transition-transform hover:scale-110 cursor-pointer"
          :class="{ 'ring-2 ring-blue-400': currentLanguage === 'en' }"
      >
        <img src="@/assets/gb-flag4x3.svg" alt="English" class="w-8 h-6" />
      </button>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
const { locale } = useI18n();
const currentLanguage = ref('sv');

const setLanguage = (lang: string) => {
  locale.value = lang;
  currentLanguage.value = lang;
  localStorage.setItem('language', lang);
};


onMounted(() => {
  const savedLanguage = localStorage.getItem('language');
  if (savedLanguage) {
    setLanguage(savedLanguage);
  }
});
</script>