  <script setup>
    import { ref } from 'vue';
    // import api from '../axios/index';
    // import { useRouter } from 'vue-router';

    // const router = useRouter();
    const input = ref({
      email: '',
      password: ''
    });

    const submit = async () => {
      try {
        console.log("Button pressed")
        console.log('Login data:', input.value);

        // Make API call to your backend
        const response = await api.post('/auth/login', input.value);

        console.log('Login response:', response.data);
        
        // Store the token
        // localStorage.setItem('token', response.data.token);
        
        // Redirect to home page after successful login
        // router.push('/home');
      } catch (error) {
        console.error('Login failed:', error);
        // Handle error (show message, etc.)
      }
    };
  </script>


<template>
    <div class="flex items-center justify-center min-h-screen bg-gray-100">
      <div class="w-full max-w-md p-8 bg-white shadow-md rounded-lg">
        <h2 class="text-2xl font-semibold text-center text-gray-700">Iniciar Sesión</h2>
        
        <form class="mt-6" @submit.prevent="submit">
          <div>
            <label class="block text-gray-700">Correo Electrónico</label>
            <input v-model="input.email" type="email" class="w-full px-4 py-2 mt-2 border rounded-lg focus:outline-none focus:ring focus:ring-blue-200" required/>
          </div>
  
          <div class="mt-4">
            <label class="block text-gray-700">Contraseña</label>
            <input v-model="input.password" type="password" class="w-full px-4 py-2 mt-2 border rounded-lg focus:outline-none focus:ring focus:ring-blue-200" required/>
          </div>
  
          <button type="submit" class="w-full px-4 py-2 mt-6 text-white bg-blue-500 rounded-lg hover:bg-blue-600">Iniciar Sesión</button>
        </form>
  
        <p class="mt-4 text-sm text-center text-gray-600">
          ¿No tienes cuenta?  
          <router-link to="/register" class="text-blue-500 hover:underline">Regístrate aquí</router-link>
        </p>
      </div>
    </div>
  </template>
  
  