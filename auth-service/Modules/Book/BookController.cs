using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Modules.Books.Controllers;

[ApiController]
[Route("books")]
public class BookController : ControllerBase
{
    // USER endpoint
    [Authorize(Roles = "user")]
    [HttpGet("user")]
    public IActionResult GetUserBook()
    {
        var book = new
        {
            Id = 1,
            Title = "Clean Code",
            Author = "Robert C. Martin",
        };

        return Ok(book);
    }

    // ADMIN endpoint
    [Authorize(Roles = "admin")]
    [HttpGet("admin")]
    public IActionResult GetAdminBook()
    {
        var book = new
        {
            Id = 1,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            CostPrice = 10,
            SellingPrice = 30,
        };

        return Ok(book);
    }
}
