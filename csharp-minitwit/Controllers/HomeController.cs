using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using csharp_minitwit.Models;
using csharp_minitwit.Services;

namespace csharp_minitwit.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDatabaseService _databaseService;

    public HomeController(ILogger<HomeController> logger, IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        _logger = logger;
    }
    /// <summary>
    /// Shows a users timeline or if no user is logged in it will redirect to the public timeline.
    /// This timeline shows the user's messages as well as all the messages of followed users.
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays the latest messages of all users.
    /// </summary>
    [HttpGet("/public")]
    public async Task<IActionResult> PublicTimeline()
    {
        var sqlQuery = @"
            SELECT message.*, user.*
            FROM message
            INNER JOIN user ON message.author_id = user.user_id
            WHERE message.flagged = 0
            ORDER BY message.pub_date DESC
            LIMIT @PerPage";
        var queryResult = await _databaseService.QueryDb(sqlQuery);
        return Ok(queryResult);
    }

    /// <summary>
    /// Logs the user in.
    /// </summary>
    [HttpGet("/login"), HttpPost("/login")]
    public async Task<IActionResult> Login()
    {
        throw new NotImplementedException();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}