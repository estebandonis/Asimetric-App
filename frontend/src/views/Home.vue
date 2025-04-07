<script setup>
  import { ref } from 'vue';
  import { useUser } from '../stores';

  const user = useUser()

  console.log("User Store", user.email)

  // Datos simulados de archivos
  const files = ref([
    { id: 1, name: 'documento.pdf' },
    { id: 2, name: 'informe.docx' },
  ]);
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <nav class="p-4 bg-blue-500 text-white flex justify-between">
      <h1 class="text-xl font-bold">Dashboard</h1>
      <button class="px-4 py-2 bg-red-500 rounded-lg hover:bg-red-600">Cerrar Sesión</button>
    </nav>

    <div class="container mx-auto p-6">
      <h2 class="text-2xl font-semibold">Administración de Archivos</h2>

      <div class="mt-6">
        <button class="px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-600">
          Generar Llaves Privadas/Públicas
        </button>
      </div>

      <div class="mt-6">
        <h3 class="text-lg font-semibold">Subir Archivo</h3>
        <input type="file" class="mt-2 p-2 border rounded-lg w-full" />
        <div class="mt-4 flex gap-4">
          <button class="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600">Guardar</button>
          <button class="px-4 py-2 bg-purple-500 text-white rounded-lg hover:bg-purple-600">Firmar y Guardar</button>
        </div>
      </div>

      <div class="mt-6">
        <h3 class="text-lg font-semibold">Archivos Disponibles</h3>
        <ul class="mt-2 bg-white p-4 rounded-lg shadow">
          <li v-for="file in files" :key="file.id" class="flex justify-between py-2 border-b">
            <span>{{ file.name }}</span>
            <div>
              <button class="px-3 py-1 bg-green-500 text-white rounded-lg hover:bg-green-600">Validar Firma</button>
              <button class="ml-2 px-3 py-1 bg-blue-500 text-white rounded-lg hover:bg-blue-600">Descargar</button>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>
