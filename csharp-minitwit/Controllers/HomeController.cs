using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using csharp_minitwit.Models;
using csharp_minitwit.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http;

namespace csharp_minitwit.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDatabaseService _databaseService;
    private readonly IConfiguration _configuration;
    private readonly string _perPage;

    public HomeController(ILogger<HomeController> logger, IDatabaseService databaseService, IConfiguration configuration)
    {
        _databaseService = databaseService;
        _logger = logger;
        _configuration = configuration;
        _perPage = configuration.GetValue<string>("Constants:PerPage")!;
    }
    /// <summary>
    /// Shows a users timeline or if no user is logged in it will redirect to the public timeline.
    /// This timeline shows the user's messages as well as all the messages of followed users.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        
        // TODO: Correctly check whether there is a logged user when register and login functionalities are working, and uncomment Redirect.
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("user_id")))
        {
            return Redirect("/public");
        }
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

        var dict = new Dictionary<string, object> {{"@PerPage", _perPage}};
        var queryResult = await _databaseService.QueryDb<dynamic>(sqlQuery, dict);

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
    public async Task<IActionResult> Login([FromForm] LoginViewModel model)
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("user_id"))) {
            return Redirect("/");
        }
        string error = null;
        if (Request.Method == "POST")
        {
            var query = "SELECT * FROM user WHERE username = @Username";
            var dict = new Dictionary<string, object> {{"@Username", model.Username}};
            var users = await _databaseService.QueryDb<UserModel>(query, dict);
            var user = users.FirstOrDefault();

            if (!users.Any()) {
                error = "Invalid username";
            } else if (model.Password.GetHashCode().ToString() != user.pw_hash) {
                error = "Invalid password";
            } else {
                HttpContext.Session.SetInt32("user_id", user.user_id);
                return Redirect("/public");
            }
        }
        if (!string.IsNullOrEmpty(error)) {
            ModelState.AddModelError("", error); // Add error to entire form
        }
        return View("login");
    }

    /// <summary>
    /// Logs the user out.
    /// </summary>
    /// <returns></returns>
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("user_id");
        return Redirect("/public");
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
    public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
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
                Console.WriteLine("Attempting to insert user into database");
                var result = await InsertUser(model.Username, model.Email, model.Password);
                Console.WriteLine("AFinished inserting user into database");
                TempData["SuccessMessage"] = "You were successfully registered and can login now";
                return RedirectToAction("Login");
            }
        }
        // If model state is not valid, return back to the registration form with validation errors
        return View(model);
    }

    private async Task<bool> IsUsernameTaken(string username)
    {
        var sqlQuery = "SELECT * FROM user WHERE username = @Username";
        var parameters = new Dictionary<string, object> {{"@Username", username}};
        var result = await _databaseService.QueryDb<dynamic>(sqlQuery, parameters);
        return result.Count() > 0;
    }

    private async Task<dynamic> InsertUser(string username, string email, string password)
    {
        var sqlQuery = @"
            INSERT INTO user (username, email, pw_hash)
            VALUES (@Username, @Email, @Password)";
        var parameters = new Dictionary<string, object>
        {
            { "@Username", username },
            { "@Email", email },
            { "@Password", password.GetHashCode() } // Not a safe way to hash passwords.
        };
        return await _databaseService.QueryDb<dynamic>(sqlQuery, parameters);
    }
}