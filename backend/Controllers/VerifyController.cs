using System.Security.Cryptography;
using System.Text;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;
public class VerifyRequest
{
    public string base64FileContent { get; set; }
    public string base64Signature { get; set; }
    public string publicKeyPem { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class VerifyController : Controller
{
    [HttpPost("signature")]
    public IActionResult VerifySignature([FromBody] VerifyRequest request)
    {
        try
        {
            byte[] fileContent = Convert.FromBase64String(request.base64FileContent);
            byte[] signature = Convert.FromBase64String(request.base64Signature);
            string publicKey = request.publicKeyPem;
            
            Console.WriteLine($"Public key: {publicKey}");

            using (RSA rsa = RSA.Create())
            {
                rsa.ImportFromPem(publicKey.ToCharArray());
                bool isValid = rsa.VerifyData(fileContent, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

                return Ok(new
                {
                    isSuccess = true,
                    isSignatureValid = isValid,
                    message = isValid ? "Signature is valid." : "Signature is invalid!"
                });
            }
        }
        catch (FormatException fo)
        {
            Console.WriteLine("Invalid base64 format." + fo.Message);
            return BadRequest(new
            {
                isSuccess = false,
                message = "Invalid base64 content or signature format.: "
            });
        }
        catch (CryptographicException ce)
        {
            Console.WriteLine("Cryptographic error: " + ce.Message);
            return BadRequest(new
            {
                isSuccess = false,
                message = $"Cryptographic error: {ce.Message}"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Internal error: " + ex.Message);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = $"Internal error: {ex.Message}"
            });
        }
    }
}
