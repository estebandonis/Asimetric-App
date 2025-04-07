import { usePrivateKey } from "../stores";

function ab2str(buf: ArrayBuffer) {
  return String.fromCharCode.apply(null, Array.from(new Uint8Array(buf)));
}

function str2ab(str: string) {
  const buf = new ArrayBuffer(str.length);
  const bufView = new Uint8Array(buf);
  for (let i = 0; i < str.length; i++) {
    bufView[i] = str.charCodeAt(i);
  }
  return buf;
}

export const generateRSAKeys = async() => {
  try {
    const privateKey = usePrivateKey();

    // Generate key pair for signing/verification
    const { publicKey: signingPublicKey, privateKey: signingPrivateKey } = await window.crypto.subtle.generateKey(
        {
          name: "RSA-PSS",
          modulusLength: 2048,
          publicExponent: new Uint8Array([0x01, 0x00, 0x01]),
          hash: { name: "SHA-256" },
        },
        true,
        ["sign", "verify"]
    );

    const exportedSigningPublic = await window.crypto.subtle.exportKey("spki", signingPublicKey);
    const exportedSigningPublicBase64 = btoa(ab2str(exportedSigningPublic));

    const exportedSigningPrivate = await window.crypto.subtle.exportKey("pkcs8", signingPrivateKey);
    const exportedSigningPrivateBase64 = btoa(ab2str(exportedSigningPrivate));

    const encrypt = await window.crypto.subtle.generateKey(
      {
        name: "AES-CBC",
        length: 256,
      },
      true,
      ["encrypt", "decrypt"]
    );  

    const exportedEncrypt = await window.crypto.subtle.exportKey("raw", encrypt);
    const exportedEncryptBase64 = btoa(ab2str(exportedEncrypt));

    // Store private keys
    const storedKeys = {
        encryption: exportedEncryptBase64,
        signing: exportedSigningPrivateBase64,
    };
    
    localStorage.setItem('privateKeys', JSON.stringify(storedKeys));
    
    privateKey.setPrivateKeys(signingPrivateKey, encrypt);

    return { 
      publicKey: exportedSigningPublicBase64,
      encryptKey: exportedEncryptBase64, 
    };
  } catch (error) {
    console.log("Error generating RSA keys:", error); 
  } 
}

export const generateECCKeys = async() => {
  try {
    const privateKey = usePrivateKey();

    // Generate ECC key pair specifically for signing/verification
    const { publicKey: signingPublicKey, privateKey: signingPrivateKey } = await window.crypto.subtle.generateKey(
        {
          name: "ECDSA",
          namedCurve: "P-256",
        },
        true,
        ["sign", "verify"]
    );

    const exportedSigningPublic = await window.crypto.subtle.exportKey("spki", signingPublicKey);
    const exportedSigningPublicBase64 = btoa(ab2str(exportedSigningPublic));

    const exportedSigningPrivate = await window.crypto.subtle.exportKey("pkcs8", signingPrivateKey);
    const exportedSigningPrivateBase64 = btoa(ab2str(exportedSigningPrivate));

    const encrypt = await window.crypto.subtle.generateKey(
        {
          name: "AES-CBC",
          length: 256,
        },
        true,
        ["encrypt", "decrypt"]
    );

    const exportedEncrypt = await window.crypto.subtle.exportKey("raw", encrypt);
    const exportedEncryptBase64 = btoa(ab2str(exportedEncrypt));

    // Store private keys
    const storedKeys = {
      encryption: exportedEncryptBase64,
      signing: exportedSigningPrivateBase64,
    };

    localStorage.setItem('privateKeys', JSON.stringify(storedKeys));
    privateKey.setPrivateKeys(signingPrivateKey, encrypt);

    return {
      publicKey: exportedSigningPublicBase64, 
      encryptKey: exportedEncryptBase64 
    }; 
  } catch (error) {
    console.log("Error generating ECC keys:", error);
  }
}

export async function encryptWithAESCBC(message: string) {
  try {
    // Import the key
    const privateKey = usePrivateKey();
    let storedKeys;

    if (privateKey.encryptPrivateKey) {
      storedKeys = privateKey.encryptPrivateKey;
    } else {
      throw new Error("Private key not found");
    }
    
    // Generate random IV
    const iv = window.crypto.getRandomValues(new Uint8Array(16));
    
    // Encode the message
    const encoder = new TextEncoder();
    const messageData = encoder.encode(message);
    
    // Encrypt
    const encrypted = await window.crypto.subtle.encrypt(
      {
        name: "AES-CBC",
        iv: iv
      },
      storedKeys,
      messageData
    );
    
    // Combine IV and ciphertext for transmission
    const combined = new Uint8Array(iv.length + encrypted.byteLength);
    combined.set(iv);
    combined.set(new Uint8Array(encrypted), iv.length);
    
    // Return as base64 string
    return btoa(ab2str(combined.buffer));
  } catch (error) {
    console.error("Encryption error:", error);
    throw error;
  }
}

export const restoreKeysFromStorage = async () => {
  try {
    const privateKey = usePrivateKey();
    const storedKeysJson = localStorage.getItem('privateKeys');
    
    if (!storedKeysJson) {
      console.log("No stored keys found");
      return null;
    }
    
    const storedKeys = JSON.parse(storedKeysJson);
    
    // Import the signing private key
    const signingPrivateKeyBuffer = str2ab(atob(storedKeys.signing));
    const signingPrivateKey = await window.crypto.subtle.importKey(
      "pkcs8",
      signingPrivateKeyBuffer,
      {
        name: "RSA-PSS",
        hash: { name: "SHA-256" },
      },
      true,
      ["sign"]
    );
    
    // Import the AES encryption key
    const encryptKeyBuffer = str2ab(atob(storedKeys.encryption));
    const encryptKey = await window.crypto.subtle.importKey(
      "raw",
      encryptKeyBuffer,
      {
        name: "AES-CBC",
      },
      true,
      ["encrypt", "decrypt"]
    );
    
    // Store the imported keys in the Pinia store
    privateKey.setPrivateKeys(signingPrivateKey, encryptKey);
  } catch (error) {
    console.log("Error restoring keys:", error);
    return null;
  }
}