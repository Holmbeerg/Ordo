<template>
  <div class="flex flex-col items-center min-h-screen">
    <h1 class="text-6xl font-bold text-center my-8 text-white">Ordo</h1>

    <div class="max-w-2xl text-center mb-8">
      <p class="text-xl mb-4 text-white">
        {{ t('home.description') }}
      </p>
    </div>

    <div v-if="!searching" class="w-full max-w-md text-center mb-8">
      <p class="text-xl mb-4 text-white">{{t('home.pickTime')}}</p>

      <div class="grid grid-cols-2 gap-4">
        <button
            @click="startSearch('bullet')"
            class="bg-red-600 text-white px-6 py-3 rounded-lg hover:bg-red-800 cursor-pointer transition duration-300"
        >
          Bullet (1+1)
        </button>

        <button
            @click="startSearch('blitz')"
            class="bg-yellow-600 text-white px-6 py-3 rounded-lg hover:bg-yellow-800 cursor-pointer transition duration-300"
        >
          Blitz (3+0)
        </button>

        <button
            @click="startSearch('rapid')"
            class="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-800 cursor-pointer transition duration-300"
        >
          Rapid (5+0)
        </button>

        <button
            @click="startSearch('classical')"
            class="bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-800 cursor-pointer transition duration-300"
        >
          {{t('home.classical')}}
        </button>
      </div>
    </div>

    <div v-else class="flex flex-col items-center min-h-screen">
      <div class="w-16 h-16 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto mb-6"></div>
      <p class="text-xl text-white mb-2">{{t('home.searching')}}</p>
      <p class="text-white mb-4">{{t('home.timeMode')}} {{ formatTimeControl(currentMode) }}</p>
      <p class="text-lg text-white mb-4">{{ formatSearchTime(searchTimeSeconds) }}</p>

      <button
          @click="cancelSearch"
          class="bg-red-600 text-white px-6 py-2 rounded-lg hover:bg-red-800 cursor-pointer transition duration-300"
      >
        {{ $t('home.cancel') }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, onUnmounted} from 'vue';
import {useRouter} from 'vue-router';
import { useI18n } from 'vue-i18n';
// TODO: import { joinQueue, leaveQueue } from '../services/socket' or something;

const router = useRouter();
const searching = ref(false);
const { t } = useI18n();
const currentMode = ref('');
const searchTimeSeconds = ref(0);
let searchTimer: number | null = null;

const formatTimeControl = (mode: string): string => { // arrow function
  switch (mode) {
    case 'bullet':
      return 'Bullet (1+1)';
    case 'blitz':
      return 'Blitz (3+0)';
    case 'rapid':
      return 'Rapid (5+0)';
    case 'classical':
      return t('home.classical');
    default:
      return mode;
  }
};

const formatSearchTime = (seconds: number): string => {
  if (seconds < 60) {
    return `${t('home.searchingFor')} ${seconds} ${t('home.seconds')}`;
  }
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;
  return `${t('home.searchingFor')} ${minutes}:${remainingSeconds < 10 ? '0' : ''}${remainingSeconds}`;
};

const startSearch = (mode: string) => {
  searching.value = true;
  currentMode.value = mode;
  searchTimeSeconds.value = 0;

  searchTimer = window.setInterval(() => {
    searchTimeSeconds.value++;
  }, 1000);

  // TODO: connect to matchmaking through Socket.IO?
  // joinQueue(mode);
};

const cancelSearch = () => {
  if (searchTimer !== null) {
    clearInterval(searchTimer);
    searchTimer = null;
  }
  searching.value = false;
  currentMode.value = '';
  searchTimeSeconds.value = 0;

  // TODO: disconnect from matchmaking through Socket.IO and connect to game?
  // leaveQueue();
};


onUnmounted(() => {
  if (searchTimer !== null) {
    clearInterval(searchTimer);
  }
});
</script>