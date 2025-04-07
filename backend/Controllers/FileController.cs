using System.Text;
using backend.Algorithms;
using backend.Models;
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
            
            // var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == file.userEmail);
            //
            // if (user == null)
            //     return StatusCode(StatusCodes.Status404NotFound, new { isSuccess = false, message = "User not found" });
            //
            // Console.WriteLine("User email: " + user.email);
            
            var hashedContent = SHAImplementation.Hash(file.fileContent);
            
            Console.WriteLine("File content: " + hashedContent);
            
            var fileToSave = new backend.Models.File
            {
                user_id = 6,
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
}