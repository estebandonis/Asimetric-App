<script setup>
  import { ref } from 'vue';
  import { useUser, usePrivateKey } from '../stores';
  import { computeSHA256, signFile } from '../utils';
  import api from '../axios/index';

  const user = useUser()
  const privateKey = usePrivateKey()

  console.log("User Store", user.email)

  const input = ref({
    fileName: '',
    file: null
  });

  function handleFileUpload(event) {
    // Get the first file from the event
    input.value.file = event.target.files[0];
    // Set the fileName property to the name of the file
    if (input.value.file) {
      input.value.fileName = input.value.file.name;
    }
    console.log("File selected:", input.value.file);
  }

  // Function to save the file
  async function saveFile() {
    if (!input.value.file) {
      alert("Please select a file first");
      return;
    }

    const fileBase64 = await readFileAsBase64(input.value.file);

    const hashedFile = computeSHA256(fileBase64);

    const signedFile = signFile(hashedFile)

    const response = await api.post('/api/file', {
      fileName: input.value.fileName,
      fileContent: fileBase64,
      hashedFile: signedFile,
      userEmail: user.email
    });
    
    console.log('File saved:', response.data);
  }

  function readFileAsBase64(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      
      reader.onload = () => {
        // Get the Base64 string (remove the data URL prefix)
        const base64String = reader.result.toString().split(',')[1];
        resolve(base64String);
      };
      
      reader.onerror = (error) => {
        reject(error);
      };
      
      reader.readAsDataURL(file);
    });
  }

</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <nav class="p-4 bg-blue-500 text-white flex justify-between">
      <h1 class="text-xl font-bold">Bienvenido {{ user.name || user.email }}</h1>

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
        <input v-model="input.fileName" type="text" placeholder="Nombre del Archivo" class="mt-2 p-2 border rounded-lg w-full" />
        <input @change="handleFileUpload" type="file" class="mt-2 p-2 border rounded-lg w-full" />
        <div class="mt-4 flex gap-4">
          <button @click="saveFile" class="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600">Guardar</button>
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
