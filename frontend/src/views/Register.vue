<script setup>
  import { ref } from 'vue';
  import api from '../axios/index';
  import { useUser } from '../stores';
  import { useRouter } from 'vue-router';
  import { generateRSAKeys, generateECCKeys } from '../utils';

  const router = useRouter();
  const user = useUser()

  const input = ref({
    email: '',
    password: '',
    confirmPassword: ''
  });

  const submit = async () => {
    try {
      console.log("Button pressed")
      console.log('Login data:', input.value);

      const { publicKey, encryptKey, keyType } = await generateRSAKeys();
      console.log('Public Key:', publicKey);
      console.log('Encrypt Key:', encryptKey);

      // Make API call to your backend
      const response = await api.post('/api/user', {
        email: input.value.email,
        password: input.value.password,
        public_key: publicKey,
        encrypt_key: encryptKey,
        key_type: keyType
      });

      console.log('Login response:', response.data);

      user.setEmail(input.value.email)
      
      // Store the token
      // localStorage.setItem('token', response.data.token);
      
      // Redirect to home page after successful login
      router.push('/home');
    } catch (error) {
      console.error('Login failed:', error);
      // Handle error (show message, etc.)
    }
  };
</script>

<template>
    <div class="flex items-center justify-center min-h-screen bg-gray-100">
      <div class="w-full max-w-md p-8 bg-white shadow-md rounded-lg">
        <h2 class="text-2xl font-semibold text-center text-gray-700">Crear Cuenta</h2>
        
        <form class="mt-6" @submit.prevent="submit">
          <div>
            <label class="block text-gray-700">Correo Electrónico</label>
            <input v-model="input.email" type="email" class="w-full px-4 py-2 mt-2 border rounded-lg focus:outline-none focus:ring focus:ring-blue-200" />
          </div>
  
          <div class="mt-4">
            <label class="block text-gray-700">Contraseña</label>
            <input v-model="input.password" type="password" class="w-full px-4 py-2 mt-2 border rounded-lg focus:outline-none focus:ring focus:ring-blue-200" />
          </div>
  
          <div class="mt-4">
            <label class="block text-gray-700">Confirmar Contraseña</label>
            <input v-model="input.confirmPassword" type="password" class="w-full px-4 py-2 mt-2 border rounded-lg focus:outline-none focus:ring focus:ring-blue-200" />
          </div>
  
          <button class="w-full px-4 py-2 mt-6 text-white bg-green-500 rounded-lg hover:bg-green-600">Registrarse</button>
        </form>
  
        <p class="mt-4 text-sm text-center text-gray-600">
          ¿Ya tienes cuenta?  
          <router-link to="/" class="text-blue-500 hover:underline">Inicia sesión aquí</router-link>
        </p>
      </div>
    </div>
  </template>
  