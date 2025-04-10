using System.Text;
using backend.Algorithms;
using backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace backend.Controllers;

public class FileRequest
{
    public string fileName { get; set; }
    public string fileContent { get; set; }
    public string userEmail { get; set; }
    public bool isSigned { get; set; }
    public string? signature { get; set; }  // Agrega esta l�nea para la firma

}


[ApiController]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
public class FileController : Controller
{
    private readonly AppDbContext _dbContext;

    public FileController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetFiles()
    {
        try
        {
            var files = await _dbContext.Files.ToListAsync();
            return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, files });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public byte[] SignFile(byte[] fileContent, string privateKey)
    {
        try
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportFromPem(privateKey.ToCharArray());
                return rsa.SignData(fileContent, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Firma Error] {ex.Message}");
            return null;
        }
    }


    [HttpPost]
    public async Task<ActionResult<IEnumerable<User>>> SaveFile([FromBody] FileRequest file)
    {
        try
        {
            byte[] fileBytes = Convert.FromBase64String(file.fileContent);

            // Verifica que la firma est� presente
            // byte[] signature = string.IsNullOrEmpty(file.signature)
            //     ? new byte[0]  // Firma vac�a si no se env�a firma
            //     : Convert.FromBase64String(file.signature);  // Convierte la firma desde Base64

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == file.userEmail);

            if (user == null)
                return StatusCode(StatusCodes.Status404NotFound, new { isSuccess = false, message = "User not found" });

            var hashedContent = SHAImplementation.Hash(file.fileContent);

            var fileToSave = new backend.Models.File
            {
                user_id = user.id,
                name = file.fileName,
                hashed_content = hashedContent,
                content = fileBytes,
                signature = file.signature  // Aqu� guardas la firma en la columna signature
            };

            _dbContext.Files.Add(fileToSave);
            await _dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new { isSuccess = true, message = "File added successfully" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = "Internal server error" });
        }
    }



    // se utiliza para descargar el archivo, el hash y la llave p�blica del usuario
    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadPackage(int fileId)
    {
        var file = await _dbContext.Files.FindAsync(fileId);
        if (file == null)
            return NotFound();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == file.user_id);
        if (user == null)
            return NotFound(new { isSuccess = false, message = "User not found for this file." });

        var zipStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            // Archivo original
            var originalEntry = archive.CreateEntry(file.name);
            using (var entryStream = originalEntry.Open())
            using (var fileStream = new MemoryStream(file.content))
            {
                await fileStream.CopyToAsync(entryStream);
            }

            // Firma del archivo (en base64)
            var signatureEntry = archive.CreateEntry("signature.txt");
            using (var writer = new StreamWriter(signatureEntry.Open()))
            {
                // string base64Signature = Convert.ToBase64String(file.signature);
                await writer.WriteAsync(file.signature);
            }


            // Llave p�blica real del usuario
            var publicKeyEntry = archive.CreateEntry("publicKey.pem");
            using (var writer = new StreamWriter(publicKeyEntry.Open()))
            {
                await writer.WriteAsync(user.public_key);
            }
        }

        zipStream.Position = 0;
        return File(zipStream, "application/zip", $"{file.name}_package.zip");
    }


}