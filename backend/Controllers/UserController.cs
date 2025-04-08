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

public class userKeyUpdate
{
    public string email { get; set; }
    public string public_key { get; set; }
    public string encrypt_key { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly JWTImplementation _jwtImplementation;

    public UserController(AppDbContext dbContext, JWTImplementation jwtImplementation)
    {
        _dbContext = dbContext;
        _jwtImplementation = jwtImplementation;
    }

    [HttpPost("login")]
    public async Task<ActionResult<IEnumerable<User>>> SignInUser([FromBody] LoginRequest user)
    {
        try
        {
            user.password = SHAImplementation.Hash(user.password);
            
            var loggedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == user.email && u.password == user.password);

            if (loggedUser == null)
                return StatusCode(StatusCodes.Status401Unauthorized, new { isSuccess = false, message = "Invalid credentials" });
            
            return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, user = loggedUser.email, token = _jwtImplementation.generarJWT(loggedUser) });
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

    [HttpPost]
    public async Task<ActionResult<User>> UpdateUser([FromBody] userKeyUpdate user)
    {
        try
        {
            var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.email == user.email);
            if (foundUser == null)
                return StatusCode(StatusCodes.Status404NotFound, new { isSuccess = false, message = "User not found" });

            foundUser.public_key = user.public_key;
            foundUser.encrypt_key = user.encrypt_key;
            
            _dbContext.Users.Update(foundUser);
            await _dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "User updated successfully" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}