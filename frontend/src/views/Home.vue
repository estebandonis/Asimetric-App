<script setup>
  import { ref, onMounted } from 'vue';
  import { useUser } from '../stores';
  import api from '../axios/index';


  const user = useUser()

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

    const response = await api.post('/api/file', {
      fileName: input.value.fileName,
      fileContent: fileBase64,
      userEmail: user.email
    });
    
    console.log('File saved:', response.data);
  }
  // funcion para obtener los archivos y mostrarlos en la lista
  const files = ref([]);

  onMounted(() => {
    fetchFiles();
  });

  async function fetchFiles() {
    try {
      const response = await api.get('/api/file');
      if (response.data.isSuccess) {
        files.value = response.data.files;
        console.log("Archivos recibidos:", files.value);
      }
    } catch (error) {
      console.error("Error al obtener los archivos:", error);
    }
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

  // verificar los jwt 

  // funcion para cerrar sesion
  function handleLogout() {
  localStorage.removeItem("token");
  localStorage.removeItem("userEmail");
  window.location.href = "/";
}

  // mostrar el formulario de validacion de firma
  const showValidationForm = ref(false);
  function toggleValidationForm() {
  showValidationForm.value = !showValidationForm.value;
}

  const publicKeyFile = ref(null);
  const hashFile = ref(null);
  const originalFile = ref(null);

  // Function to handle file uploads for public key, hash, and original file
  function handlePublicKeyUpload(e) {
    publicKeyFile.value = e.target.files[0];
  }

  function handleHashUpload(e) {
    hashFile.value = e.target.files[0];
  }

  function handleOriginalFileUpload(e) {
    originalFile.value = e.target.files[0];
  }
/*
  async function verifySignature() {
    if (!publicKeyFile.value || !hashFile.value || !originalFile.value) {
      alert("Por favor sube todos los archivos");
      return;
    }

    const formData = new FormData();
    formData.append('publicKey', publicKeyFile.value);
    formData.append('hash', hashFile.value);
    formData.append('originalFile', originalFile.value);

    try {
      const response = await api.post('/api/file/verify-signature', formData);
      alert(response.data.message || "Verificación completada");
    } catch (error) {
      console.error("Error verificando firma:", error);
      alert("Error verificando firma");
    }
  }*/

  async function downloadFile(fileId) {
  try {
    const response = await api.get(`/api/file/download/${fileId}`, {
      responseType: 'blob'
    });

    const blob = new Blob([response.data], { type: 'application/zip' });
    const url = window.URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = url;
    link.download = `archivo_${fileId}.zip`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  } catch (error) {
    console.error("Error al descargar:", error);
  }
}


</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <nav class="p-4 bg-blue-500 text-white flex justify-between">
      <h1 class="text-xl font-bold">Bienvenido {{ user.name || user.email }}</h1>

      <button
        @click="handleLogout"
        class="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600 transition"
      >
        Cerrar sesión
      </button>
    </nav>

    <div class="container mx-auto p-6">
      <h2 class="text-2xl font-semibold">Administración de Archivos</h2>

      <div class="mt-6">
        <button class="px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-600">
          Generar Llaves Privadas/Públicas
        </button>

        <button @click="toggleValidationForm" class="px-4 py-2 bg-yellow-500 text-white rounded-lg hover:bg-yellow-600">
          Validar Firma
        </button>
      </div>

      <!-- Mostrar Inputs Condicionalmente -->
      <div v-if="showValidationForm" class="mt-4 space-y-4">
        <input type="file" @change="handlePublicKeyUpload" accept=".pem" class="block w-full p-2 border rounded-lg" placeholder="Llave Pública .pem" />
        <input type="file" @change="handleHashUpload" accept=".txt" class="block w-full p-2 border rounded-lg" placeholder="Hash .txt" />
        <input type="file" @change="handleOriginalFileUpload" class="block w-full p-2 border rounded-lg" placeholder="Archivo original" />

        <button @click="verifySignature" class="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600">Verificar Firma</button>
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
              <button @click="downloadFile(file.id)" class="ml-2 px-3 py-1 bg-blue-500 text-white rounded-lg hover:bg-blue-600">Descargar</button>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>
