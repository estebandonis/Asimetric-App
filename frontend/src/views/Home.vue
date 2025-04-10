<script setup>
  import { ref, onMounted } from 'vue';
  import { useUser, usePrivateKey } from '../stores';
  import { signFile, restoreKeysFromStorage } from '../utils';
  import api from '../axios/index';


  const user = useUser()
  const privateKey = usePrivateKey()

  console.log("User Store", user.email)
  console.log("Aqui",localStorage.privateKeys)

  onMounted(async () => {
  try {
    await restoreKeysFromStorage();
    await fetchFiles();
  } catch (error) {
    console.error("Error initializing:", error);
  }
});
  

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

  if (!user.email) {
    alert("User not logged in");
    return;
  }


    const fileBase64 = await readFileAsBase64(input.value.file);

    const response = await api.post('/api/file', {
      fileName: input.value.fileName,
      fileContent: fileBase64,
      userEmail: user.email,
      signature: null, // No signature for non-signed files
      isSigned: false
    });
    
    console.log('File saved:', response.data);
    await fetchFiles(); // Refresh the file list after saving
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

async function signAndSaveFile() {
  if (!input.value.file) {
    alert("Por favor seleccione un archivo primero");
    return;
  }

  try {
    // 1. Obtener y validar la llave privada
    const privateKeysStr = localStorage.getItem("privateKeys");
    if (!privateKeysStr) {
      throw new Error("No se encontró la llave privada");
    }

    const keys = JSON.parse(privateKeysStr);
    if (!keys.signing) { 
      throw new Error("Formato de llave privada inválido");
    }

    // 2. Convertir el archivo a base64 primero
    const originalFileBase64 = await readFileAsBase64(input.value.file);

    // 3. Firmar el archivo usando la llave de firma
    const signature = await signFile(originalFileBase64, keys.signing); // Cambiado a keys.signing

    // Extraer el nombre del archivo y la extensión
    const fileName = input.value.fileName;
    const lastDotIndex = fileName.lastIndexOf('.');
    const name = fileName.substring(0, lastDotIndex);
    const extension = fileName.substring(lastDotIndex);
    const signedFileName = `${name}_signed${extension}`;
    // 4. Crear el objeto para enviar al servidor
    const response = await api.post('/api/file', {
      fileName: signedFileName,
      fileContent: originalFileBase64,
      signature: signature,
      userEmail: user.email,
      isSigned: true
    });

    console.log('Archivo firmado y guardado:', response.data);
    alert("Archivo firmado y guardado exitosamente");
    await fetchFiles();

  } catch (error) {
    console.error("Error al firmar y guardar el archivo:", error);
    alert("Error al firmar el archivo: " + error.message);
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
        <input type="file" @change="handleHashUpload" accept=".txt" class="block w-full p-2 border rounded-lg" placeholder="Firma .txt" />
        <input type="file" @change="handleOriginalFileUpload" class="block w-full p-2 border rounded-lg" placeholder="Archivo original" />

        <button @click="verifySignature" class="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600">Verificar Firma</button>
      </div>

      <div class="mt-6">
        <h3 class="text-lg font-semibold">Subir Archivo</h3>
        <input v-model="input.fileName" type="text" placeholder="Nombre del Archivo" class="mt-2 p-2 border rounded-lg w-full" />
        <input @change="handleFileUpload" type="file" class="mt-2 p-2 border rounded-lg w-full" />
        <div class="mt-4 flex gap-4">
          <button @click="saveFile" class="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600">Guardar</button>
          <button 
            class="px-4 py-2 bg-green-500 hover:bg-green-600 text-white rounded"
            @click="signAndSaveFile"
          >
            Firmar y Guardar
          </button>
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