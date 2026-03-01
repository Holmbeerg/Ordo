import type { InjectionKey, Ref } from 'vue';
import type { Tile } from '@/types/game.ts';

// Typed injection key so GameView can share the selected tile ref with
// GameBoard and TileRack without passing it as a prop through every level.
export const SELECTED_TILE_KEY: InjectionKey<Ref<Tile | null>> = Symbol('selectedTile');
