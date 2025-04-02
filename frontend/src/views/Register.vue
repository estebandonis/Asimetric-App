<template>
    <div class="flex items-center justify-center min-h-screen bg-gray-100">
      <div class="w-full max-w-md p-8 bg-white shadow-md rounded-lg">
        <h2 class="text-2xl font-semibold text-center text-gray-700">Crear Cuenta</h2>
        
        <form class="mt-6">
          <div>
            <label class="block text-gray-700">Correo Electrónico</label>
            <input type="email" class="w-full px-4 py-2 mt-2 border rounded-lg focus:outline-none focus:ring focus:ring-blue-200" />
          </div>
  
          <div class="mt-4">
            <label class="block text-gray-700">Contraseña</label>
            <input type="password" class="w-full px-4 py-2 mt-2 border rounded-lg focus:outline-none focus:ring focus:ring-blue-200" />
          </div>
  
          <div class="mt-4">
            <label class="block text-gray-700">Confirmar Contraseña</label>
            <input type="password" class="w-full px-4 py-2 mt-2 border rounded-lg focus:outline-none focus:ring focus:ring-blue-200" />
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
  
  <script setup>
  </script>
  