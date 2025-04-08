using System.Text;
using backend.Algorithms;
using backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

public class FileRequest
{
    public string fileName { get; set; }
    public string fileContent { get; set; }
    public string userEmail { get; set; }
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

    [HttpPost]
    public async Task<ActionResult<IEnumerable<User>>> SaveFile([FromBody] FileRequest file)
    {
        try
        {
            Console.WriteLine("Uploading file");

            byte[] fileBytes = Convert.FromBase64String(file.fileContent);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == file.userEmail);

            if (user == null)
                return StatusCode(StatusCodes.Status404NotFound, new { isSuccess = false, message = "User not found" });

            Console.WriteLine("User email: " + user.email);

            var hashedContent = SHAImplementation.Hash(file.fileContent);

            Console.WriteLine("File content: " + hashedContent);

            var fileToSave = new backend.Models.File
            {
                user_id = user.id, // Use the actual user ID instead of hardcoded value
                name = file.fileName,
                hashed_content = hashedContent,
                content = fileBytes
            };

            Console.WriteLine("File to save: " + fileToSave.name);

            _dbContext.Files.Add(fileToSave);
            await _dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, new { isSuccess = true, message = "File added successfully" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // se utiliza para descargar el archivo, el hash y la llave pública del usuario
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

            // Hash del archivo
            var hashEntry = archive.CreateEntry("hash.txt");
            using (var writer = new StreamWriter(hashEntry.Open()))
            {
                await writer.WriteAsync(file.hashed_content);
            }

            // Llave pública real del usuario
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