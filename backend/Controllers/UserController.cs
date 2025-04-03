using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

public class UserController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}