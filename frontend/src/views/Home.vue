<script setup>
  import { ref, onMounted } from 'vue';
  import { useUser, usePrivateKey } from '../stores';
  import api from '../axios/index';
import { encryptFile } from '../utils';


  const user = useUser()
  const privateKey = usePrivateKey()

  console.log("User Store", user.email)
  console.log("Private Key Store", privateKey.signPrivateKey)

  onMounted(async () => {
  try {
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
  const signatureFile = ref(null);
  const originalFile = ref(null);

  // Function to handle file uploads for public key, hash, and original file
  function handlePublicKeyUpload(e) {
    publicKeyFile.value = e.target.files[0];
  }

  function handleSignatureUpload(e) {
    signatureFile.value = e.target.files[0];
  }

  function handleOriginalFileUpload(e) {
    originalFile.value = e.target.files[0];
  }

  async function verifySignature() {
    if (!publicKeyFile.value || !signatureFile.value || !originalFile.value) {
      alert("Por favor sube todos los archivos");
      return;
    }

    try {
      // Leer el archivo de llave pública como texto
      const publicKeyText = await readFileAsText(publicKeyFile.value);
      
      // Asegurarse que la llave tenga el formato correcto
      const formattedPublicKey = formatAsPEM(publicKeyText);
      
      // Leer la firma como texto base64 puro
      const signatureText = await readFileAsText(signatureFile.value);

      const signatureBase64 = signatureText.trim(); // Eliminar espacios extra
      
      // Leer el archivo original como base64
      const fileContentBase64 = await readFileAsBase64(originalFile.value);

      // const encryptedFile = await encryptFile(fileContentBase64);

      const requestData = {
        base64FileContent: fileContentBase64,
        base64Signature: signatureBase64,
        publicKeyPem: formattedPublicKey
      };

      const response = await api.post('/api/Verify/signature', requestData);
      
      if (response.data.isSuccess) {
        if (response.data.isSignatureValid) {
          alert("¡Firma válida! El archivo no ha sido modificado.");
        } else {
          alert("¡Firma inválida! El archivo puede haber sido modificado.");
        }
      } else {
        throw new Error(response.data.message);
      }
    } catch (error) {
      console.error("Error completo:", error);
      alert("Error verificando firma: " + (error.response?.data?.message || error.message));
    }
}

// Función mejorada para formatear PEM
function formatAsPEM(content) {
  // Primero eliminar cualquier delimitador existente y espacios
  content = content
    .replace(/-----BEGIN PUBLIC KEY-----/g, '')
    .replace(/-----END PUBLIC KEY-----/g, '')
    .replace(/[\r\n\s]/g, '')
    .trim();
  
  // Agregar los delimitadores y formato correcto
  return `-----BEGIN PUBLIC KEY-----\n${content}\n-----END PUBLIC KEY-----`;
}

// Agregar esta función helper para leer archivos como texto
function readFileAsText(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result);
    reader.onerror = reject;
    reader.readAsText(file);
  });
}

  async function downloadFile(fileId) {
  try {
    const response = await api.get(`/api/file/download/${fileId}`, {
      responseType: 'blob'
    });

    // Obtener el contenido como texto
    const fileContent = await response.data.text();

    // Si el archivo es una llave pública (verificar por extensión o tipo)
    if (response.headers['content-type'].includes('pem')) {
      // Agregar los delimitadores PEM si no están presentes
      const pemContent = formatAsPEM(fileContent);
      
      // Crear nuevo blob con el contenido formateado
      const blob = new Blob([pemContent], { type: 'application/x-pem-file' });
      const url = window.URL.createObjectURL(blob);
      
      const link = document.createElement('a');
      link.href = url;
      link.download = `public_key_${fileId}.pem`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } else {
      // Para otros tipos de archivos, mantener el comportamiento original
      const blob = new Blob([response.data], { type: 'application/zip' });
      const url = window.URL.createObjectURL(blob);
      
      const link = document.createElement('a');
      link.href = url;
      link.download = `archivo_${fileId}.zip`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    }
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
    // 1. Convertir el archivo a base64 primero
    const originalFileBase64 = await readFileAsBase64(input.value.file);

    const fileContent = await encryptFile(originalFileBase64);

    // 2. Read file as ArrayBuffer
    const reader = new FileReader();
    const fileArrayBuffer = await new Promise((resolve, reject) => {
      reader.onload = () => resolve(reader.result);
      reader.onerror = reject;
      reader.readAsArrayBuffer(input.value.file);
    });

    // 3. Firmar el archivo usando la llave de firma
    const signature = await window.crypto.subtle.sign(
      {
        name: "RSA-PSS",
        saltLength: 32,
        hash: { name: "SHA-256" },
      },
      privateKey.signPrivateKey,
      fileArrayBuffer,
    );

    console.log("Firma generada:", signature);

    const signatureBase64 = btoa(String.fromCharCode(...new Uint8Array(signature)));

    console.log("Firma en Base64:", signatureBase64);

    // Extraer el nombre del archivo y la extensión
    const fileName = input.value.fileName;
    const lastDotIndex = fileName.lastIndexOf('.');
    const name = fileName.substring(0, lastDotIndex);
    const extension = fileName.substring(lastDotIndex);
    const signedFileName = `${name}_signed${extension}`;

    // 4. Crear el objeto para enviar al servidor
    const response = await api.post('/api/file', {
      fileName: signedFileName,
      fileContent: fileContent,
      signature: signatureBase64,
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
        <input type="file" @change="handleSignatureUpload" accept=".txt" class="block w-full p-2 border rounded-lg" placeholder="Firma .txt" />
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