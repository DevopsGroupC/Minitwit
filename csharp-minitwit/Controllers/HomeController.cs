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
        // TODO: Correctly check whether there is a logged user when register and login functionalities are working, and uncomment Redirect.
        // if (!User.Identity.IsAuthenticated)
        // {
        //     return Redirect("/public");
        // }

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

        var messages = queryResult.Select(row =>
    {
        var dict = (IDictionary<string, object>)row;
        return new MessageModel
        {
            MessageId = (long)dict["message_id"],
            AuthorId = (long)dict["author_id"],
            Text = (string)dict["text"],
            PubDate = (long)dict["pub_date"],
            Flagged = (long)dict["flagged"],
            UserId = (long)dict["user_id"],
            Username = (string)dict["username"],
            Email = (string)dict["email"],
            PwHash = (string)dict["pw_hash"]
        };
    }).ToList();

        return View("PublicTimeline", messages);
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

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// 
    

    [HttpGet("/register")]
    public async Task<IActionResult> Register()
    {
        // Return the view
        return View();
    }

    [HttpPost("/register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            //Validate form inputs
            if (string.IsNullOrEmpty(model.Username))
            {
                ModelState.AddModelError("Username", "You have to enter a username");
            }
            else if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "You have to enter a password");
            }
            else if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", "You have to enter an email address");
            }
            else if (model.Password != model.Password2)
            {
                ModelState.AddModelError("Password2", "The two passwords do not match");
            }
            else if (await IsUsernameTaken(model.Username))
            {
                ModelState.AddModelError("Username", "The username is already taken");
            }
            else
            {
                //Insert user into database
                await InsertUser(model.Username, model.Email, model.Password);
                TempData["SuccessMessage"] = "You were successfully registered and can login now";
                return RedirectToAction("Login");
            }
        }
        // If model state is not valid, return back to the registration form with validation errors
        return View(model);
    }

    private async Task<bool> IsUsernameTaken(string username)
    {
        throw new NotImplementedException();
    }

    private async Task InsertUser(string username, string email, string password)
    {
    /*     var sqlQuery = @"
            INSERT INTO user (username, email, pw_hash)
            VALUES (@Username, @Email, @Password)";
        var parameters = new Dictionary<string, object>
        {
            { "@Username", username },
            { "@Email", email },
            { "@Password", GeneratePasswordHash(password) }
        };
        await _databaseService.ExecuteDb(sqlQuery, parameters); */
    }
   
}