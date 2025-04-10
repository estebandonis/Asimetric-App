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

    privateKey.setPrivateKeys(signingPrivateKey, encrypt, "RSA-PSS");

    return { 
      publicKey: exportedSigningPublicBase64,
      encryptKey: exportedEncryptBase64, 
      keyType: "RSA-PSS"
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

    privateKey.setPrivateKeys(signingPrivateKey, encrypt, "ECDSA");

    return {
      publicKey: exportedSigningPublicBase64, 
      encryptKey: exportedEncryptBase64,
      keyType: "ECDSA"
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

export const encryptFile = async (fileContent) => {
  try {
    const privateKey = usePrivateKey();
    if (!privateKey.encryptPrivateKey) {
      console.error("Encryption private key not found");
      return null;
    }
    // Convert base64 to array buffer for crypto operations
    const binaryString = atob(fileContent);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    // Generate random IV
    const iv = window.crypto.getRandomValues(new Uint8Array(16));
    // Encrypt the file content
    const encrypted = await window.crypto.subtle.encrypt(
      {
        name: "AES-CBC",
        iv: iv,
      },
      privateKey.encryptPrivateKey,
      bytes.buffer,
    );
    // Combine IV and ciphertext for transmission
    const combined = new Uint8Array(iv.length + encrypted.byteLength);
    combined.set(iv);
    combined.set(new Uint8Array(encrypted), iv.length);
    // Convert to base64
    const base64Combined = btoa(ab2str(combined.buffer));
    // Convert to hex
    const hexArray = Array.from(new Uint8Array(combined));
    const hexString = hexArray.map(b => b.toString(16).padStart(2, '0')).join('');
    return { base64: base64Combined, hex: hexString };
  } catch (error) {
    console.error("Error encrypting file:", error);
    throw error;
  }
}

export const decryptFile = async (encryptedContent) => {
  try {
    const privateKey = usePrivateKey();
    if (!privateKey.encryptPrivateKey) {
      console.error("Decryption private key not found");
      return null;
    }
    // Convert base64 to array buffer for crypto operations
    const binaryString = atob(encryptedContent);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    // Extract IV from the first 16 bytes
    const iv = bytes.slice(0, 16);
    const ciphertext = bytes.slice(16);
    // Decrypt the file content
    const decrypted = await window.crypto.subtle.decrypt(
      {
        name: "AES-CBC",
        iv: iv,
      },
      privateKey.encryptPrivateKey,
      ciphertext.buffer,
    );
    // Convert decrypted content to base64
    const decryptedArray = new Uint8Array(decrypted);
    const decryptedBase64 = btoa(ab2str(decryptedArray.buffer));
    // Convert decrypted content to hex
    const hexArray = Array.from(decryptedArray);
    const hexString = hexArray.map(b => b.toString(16).padStart(2, '0')).join('');
    return { base64: decryptedBase64, hex: hexString };
  } catch (error) {
    console.error("Error decrypting file:", error);
    throw error;
  }
}

export const computeSHA256 = async (fileContent) => {
  // Convert base64 to array buffer for crypto operations
  const binaryString = atob(fileContent);
  const bytes = new Uint8Array(binaryString.length);
  for (let i = 0; i < binaryString.length; i++) {
    bytes[i] = binaryString.charCodeAt(i);
  }
  
  // Use the Web Crypto API to calculate SHA-256 hash
  const hashBuffer = await crypto.subtle.digest('SHA-256', bytes.buffer);
  
  // Convert hash to hex string
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  
  return hashHex;
}