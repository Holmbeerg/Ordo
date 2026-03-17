import { computed } from 'vue';
import { useGameStore } from '@/stores/gameStore';
import { calculatePreviewScore } from '@/utils/scoring.ts';

export function useScorePreview() {
    const gameStore = useGameStore();

    const previewScore = computed(() => {
        const placements = gameStore.pendingPlacements;
        const board = gameStore.board;

        if (!board || placements.length === 0) return 0;

        return calculatePreviewScore(board, placements);
    });

    return {
        previewScore,
    };
}
