import { createApp } from "vue";
import { createPinia } from "pinia";
import { createI18n } from 'vue-i18n';
import en from './locale/en.json';
import se from './locale/se.json';

import App from "./App.vue";
import router from "./router";
import './style.css'

const app = createApp(App);

const i18n = createI18n({
    legacy: false,
    locale: 'se',
    fallbackLocale: 'en',
    messages: {
        en,
        se
    }
});

app.use(createPinia());
app.use(i18n);
app.use(router);

app.mount("#app");
