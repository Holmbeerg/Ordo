<template>
    <div class="flex flex-col items-center min-h-screen mx-5">
        <h1 class="text-6xl font-bold text-center my-8 text-white">Ordo</h1>

        <div class="max-w-2xl text-center mb-8">
            <p class="text-xl mb-4 text-white">
                {{ t('home.description') }}
            </p>
        </div>

        <div v-if="!searching" class="w-full max-w-md text-center mb-8">
            <p class="text-xl mb-4 text-white">{{ t('home.pickTime') }}</p>

            <div class="flex flex-col gap-4 w-full">
                <div class="grid grid-cols-2 gap-4">
                    <button
                        @click="startSearch('Blitz')"
                        class="bg-yellow-600 text-white px-6 py-3 rounded-lg hover:bg-yellow-800 cursor-pointer transition duration-300"
                    >
                        Blitz (3+2)
                    </button>

                    <button
                        @click="startSearch('Rapid')"
                        class="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-800 cursor-pointer transition duration-300"
                    >
                        Rapid (8+5)
                    </button>
                </div>

                <button
                    @click="startSearch('Classical')"
                    class="w-full bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-800 cursor-pointer transition duration-300"
                >
                    {{ t('home.classical') }}
                </button>
            </div>
        </div>

        <div v-else class="flex flex-col items-center min-h-screen">
            <div
                class="w-16 h-16 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto mb-6"
            ></div>
            <p class="text-xl text-white mb-2">{{ t('home.searching') }}</p>
            <p class="text-white mb-4">
                {{ t('home.timeMode') }} {{ formatTimeControl(currentMode) }}
            </p>
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
import { useI18n } from 'vue-i18n';
import type { TimeControl } from '@/types/TimeControl.ts';
import { useMatchmaking } from '@/composables/useMatchmaking.ts';

const { t } = useI18n();
const { searching, currentMode, searchTimeSeconds, startSearch, cancelSearch } = useMatchmaking();

const formatTimeControl = (timeControl: TimeControl): string => {
    switch (timeControl) {
        case 'Blitz':
            return 'Blitz (3+0)';
        case 'Rapid':
            return 'Rapid (5+0)';
        case 'Classical':
            return t('home.classical');
        default:
            return timeControl;
    }
};

const formatSearchTime = (seconds: number): string => {
    if (seconds < 60) return `${t('home.searchingFor')} ${seconds} ${t('home.seconds')}`;
    const minutes = Math.floor(seconds / 60);
    const remaining = seconds % 60;
    return `${t('home.searchingFor')} ${minutes}:${remaining < 10 ? '0' : ''}${remaining}`;
};
</script>
