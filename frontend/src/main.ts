import { createApp } from 'vue';
import App from './App.vue';
import { createPinia } from 'pinia';
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import router from './router'; // Importa el router
import './style.css'; // Importa estilos globales si los tienes

const pinia = createPinia(); // Crea la instancia de Pinia
pinia.use(piniaPluginPersistedstate); // Usa el plugin de persistencia
const app = createApp(App);
app.use(router); // Usa Vue Router
app.use(pinia);
app.mount('#app');
