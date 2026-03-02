<script setup lang="ts">
defineProps<{
    visible: boolean;
}>();

const emit = defineEmits<{
    pick: [letter: string];
    cancel: [];
}>();

const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ'.split('');
</script>

<template>
    <Teleport to="body">
        <Transition name="modal">
            <div
                v-if="visible"
                class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm"
                @click.self="emit('cancel')"
            >
                <div
                    class="bg-[#2a2826] border border-[#3d3b38] rounded-2xl shadow-2xl p-6 flex flex-col gap-4 w-[400px] max-w-[95vw]"
                >
                    <!-- Header -->
                    <div class="flex items-center justify-between">
                        <h2 class="text-white font-semibold text-base">Choose a letter</h2>
                        <button
                            class="text-gray-400 hover:text-white text-xl leading-none cursor-pointer transition-colors"
                            @click="emit('cancel')"
                        >
                            ✕
                        </button>
                    </div>

                    <p class="text-gray-400 text-sm">
                        Pick the letter this blank tile should represent.
                    </p>

                    <!-- Letter grid -->
                    <div class="grid grid-cols-7 gap-2 justify-items-center py-2">
                        <button
                            v-for="letter in alphabet"
                            :key="letter"
                            class="w-10 h-10 flex items-center justify-center rounded font-bold text-base shadow transition-all duration-100 select-none cursor-pointer bg-[#f5e6c8] text-gray-900 hover:scale-110 hover:bg-amber-300 hover:ring-2 hover:ring-amber-400 active:scale-95"
                            @click="emit('pick', letter)"
                        >
                            {{ letter }}
                        </button>
                    </div>

                    <!-- Cancel -->
                    <div class="flex justify-center pt-1">
                        <button
                            class="px-4 py-2 rounded-lg bg-[#3d3b38] hover:bg-[#4a4846] text-gray-300 text-sm font-medium transition cursor-pointer"
                            @click="emit('cancel')"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            </div>
        </Transition>
    </Teleport>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
    transition: opacity 0.2s ease;
}
.modal-enter-from,
.modal-leave-to {
    opacity: 0;
}
</style>
