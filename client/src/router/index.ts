import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '@/views/HomeView.vue';
import GameView from '@/views/GameView.vue';

const routes = [
    { path: '/', component: HomeView },
    { path: '/game/:gameId', component: GameView, meta: { hideFooter: true, hideHeader: true } },
];

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
});

export default router;
