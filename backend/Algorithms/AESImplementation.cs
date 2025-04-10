using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace backend.Algorithms;

public class AESImplementation
{
    public static byte[] DecryptFile(string base64EncryptedData, string keyString)
    {
        try
        {
            // Convert key from Base64
            byte[] key = Convert.FromBase64String(keyString);
            
            // Convert combined data from Base64
            byte[] combinedData = Convert.FromBase64String(base64EncryptedData);
            
            // Debug information
            Console.WriteLine($"Combined data length: {combinedData.Length}");
            
            // Extract IV (first 16 bytes)
            byte[] iv = new byte[16];
            Array.Copy(combinedData, 0, iv, 0, 16);
            
            // Extract ciphertext (everything after the IV)
            byte[] encryptedData = new byte[combinedData.Length - 16];
            Array.Copy(combinedData, 16, encryptedData, 0, encryptedData.Length);
            
            // Debug information
            Console.WriteLine($"Ciphertext length: {encryptedData.Length}");
            Console.WriteLine($"IV first bytes: {BitConverter.ToString(iv, 0, 4)}");
            Console.WriteLine($"Ciphertext first bytes: {BitConverter.ToString(encryptedData, 0, 4)}");
            
            // Ensure ciphertext length is a multiple of the block size
            if (encryptedData.Length % 16 != 0)
            {
                Console.WriteLine($"WARNING: Ciphertext length ({encryptedData.Length}) is not a multiple of 16");
                // Don't throw yet - try to decrypt anyway
            }
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedData, 0, encryptedData.Length);
                        cs.FlushFinalBlock(); // Important!
                    }
                    return ms.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Decryption error details: {ex.Message}");
            throw;
        }
    }
}