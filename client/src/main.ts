import { createApp } from 'vue';
import { createPinia } from 'pinia';
import { createI18n } from 'vue-i18n';
import Toast from 'vue-toastification';
import 'vue-toastification/dist/index.css';
import en from './locale/en.json';
import se from './locale/se.json';

import App from './App.vue';
import router from './router';
import './style.css';

const app = createApp(App);

const i18n = createI18n({
    legacy: false,
    locale: 'se',
    fallbackLocale: 'en',
    messages: { en, se },
});

app.use(createPinia());
app.use(i18n);
app.use(router);
app.use(Toast, {
    position: 'bottom-right',
    timeout: 3500,
    closeOnClick: true,
    pauseOnHover: true,
    draggable: false,
    hideProgressBar: false,
    maxToasts: 5,
});

app.mount('#app');
