<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useGame } from '@/composables/useGame.ts';

const router = useRouter();
const { locale } = useI18n();
const { resignGame, isGameOver } = useGame();
const open = ref(false);
const currentLanguage = ref('en');
const confirming = ref(false);

const setLanguage = (lang: string) => {
    locale.value = lang;
    currentLanguage.value = lang;
    localStorage.setItem('language', lang);
};

onMounted(() => {
    const saved = localStorage.getItem('language');
    if (saved) {
        locale.value = saved;
        currentLanguage.value = saved;
    }
});

async function leaveGame() {
    // If game is already over just navigate away, no need to resign
    if (isGameOver.value) {
        open.value = false;
        await router.push('/');
        return;
    }
    confirming.value = true;
}

async function confirmResign() {
    confirming.value = false;
    open.value = false;
    await resignGame();
    await router.push('/');
}

function cancelResign() {
    confirming.value = false;
}
</script>

<template>
    <!-- Floating menu anchored to top-right of its positioned parent -->
    <div class="absolute top-2 right-2 z-50 select-none">
        <!-- Toggle button -->
        <button
            @click="open = !open"
            class="w-8 h-8 flex flex-col items-center justify-center gap-1 rounded-lg bg-[#3a3835]/80 hover:bg-[#4a4845] border border-[#1a1917] transition-colors cursor-pointer backdrop-blur-sm"
            title="Menu"
        >
            <!-- Hamburger icon transforms into an X when open -->
            <span
                :class="[
                    'block w-4 h-0.5 bg-gray-300 transition-all duration-200',
                    open ? 'rotate-45 translate-y-1.5' : '',
                ]"
            />
            <span
                :class="[
                    'block w-4 h-0.5 bg-gray-300 transition-all duration-200',
                    open ? 'opacity-0' : '',
                ]"
            />
            <span
                :class="[
                    'block w-4 h-0.5 bg-gray-300 transition-all duration-200',
                    open ? '-rotate-45 -translate-y-1.5' : '',
                ]"
            />
        </button>

        <!-- Dropdown -->
        <Transition name="menu">
            <div
                v-if="open"
                class="absolute top-10 right-0 w-44 bg-[#3a3835] border border-[#1a1917] rounded-xl shadow-2xl py-2 flex flex-col"
            >
                <!-- Language -->
                <div class="px-3 py-2">
                    <p class="text-[10px] text-gray-500 uppercase tracking-widest mb-2">Language</p>
                    <div class="flex items-center gap-2">
                        <button
                            @click="setLanguage('se')"
                            class="p-1 rounded transition-transform hover:scale-110 cursor-pointer"
                            :class="currentLanguage === 'se' ? 'ring-2 ring-blue-400' : ''"
                        >
                            <img src="@/assets/swedish-flag4x3.svg" alt="Swedish" class="w-8 h-6" />
                        </button>
                        <span class="text-gray-500">/</span>
                        <button
                            @click="setLanguage('en')"
                            class="p-1 rounded transition-transform hover:scale-110 cursor-pointer"
                            :class="currentLanguage === 'en' ? 'ring-2 ring-blue-400' : ''"
                        >
                            <img src="@/assets/gb-flag4x3.svg" alt="English" class="w-8 h-6" />
                        </button>
                    </div>
                </div>

                <div class="h-px bg-[#1a1917] mx-3 my-1" />

                <!-- Leave game -->
                <button
                    @click="leaveGame"
                    class="mx-2 px-3 py-2 text-sm text-red-400 hover:text-red-300 hover:bg-red-900/20 rounded-lg text-left transition-colors cursor-pointer"
                >
                    ← Leave game
                </button>
            </div>
        </Transition>

        <!-- Resign confirmation -->
        <Transition name="menu">
            <div
                v-if="confirming"
                class="absolute top-10 right-0 w-52 bg-[#3a3835] border border-red-800 rounded-xl shadow-2xl p-3 flex flex-col gap-2"
            >
                <p class="text-sm text-gray-200 font-semibold">Resign the game?</p>
                <p class="text-xs text-gray-400">Your opponent will win. This cannot be undone.</p>
                <div class="flex gap-2 mt-1">
                    <button
                        @click="confirmResign"
                        class="flex-1 px-3 py-1.5 text-xs font-semibold bg-red-700 hover:bg-red-600 text-white rounded-lg transition-colors cursor-pointer"
                    >
                        Resign
                    </button>
                    <button
                        @click="cancelResign"
                        class="flex-1 px-3 py-1.5 text-xs font-semibold bg-[#4a4845] hover:bg-[#5a5855] text-gray-200 rounded-lg transition-colors cursor-pointer"
                    >
                        Cancel
                    </button>
                </div>
            </div>
        </Transition>
    </div>
</template>

<!-- For Transition don't use scoped, https://vuejs.org/guide/built-ins/transition -->
<style>
.menu-enter-active,
.menu-leave-active {
    transition:
        opacity 0.25s ease,
        transform 0.25s ease;
}
.menu-enter-from,
.menu-leave-to {
    opacity: 0;
    transform: translateY(-4px);
}
</style>
