using backend.Algorithms;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

public class LoginRequest
{
    public string email { get; set; }
    public string password { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly AppDbContext _dbContext;

    public UserController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<ActionResult<IEnumerable<User>>> SignInUser([FromBody] LoginRequest user)
    {
        try
        {
            Console.WriteLine("Sign in User Email: ");
            
            user.password = SHAImplementation.Hash(user.password);
            
            var loggedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == user.email && u.password == user.password);
            
            if (loggedUser == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { isSuccess = false, message = "Invalid credentials" });
            
            return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, user = loggedUser.email });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        try
        {
            Console.WriteLine("User Email: " + user.email);
            Console.WriteLine("User Password: " + user.password);
            Console.WriteLine("User key: " + user.public_key);
        
            user.password = SHAImplementation.Hash(user.password);
        
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, new { isSuccess = true, message = "User created successfully", user.email });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}