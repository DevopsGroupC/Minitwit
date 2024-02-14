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
    private readonly IDatabaseService _databaseService;
    private readonly IConfiguration _configuration;
    private readonly string _perPage;

    public HomeController( IDatabaseService databaseService, IConfiguration configuration)
    {
        _databaseService = databaseService;
        _configuration = configuration;
        _perPage = configuration.GetValue<string>("Constants:PerPage")!;
    }
    /// <summary>
    /// Shows a users timeline or if no user is logged in it will redirect to the public timeline.
    /// This timeline shows the user's messages as well as all the messages of followed users.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult>  Timeline()
    {
        var userId = HttpContext.Session.GetInt32("user_id");

        if (!userId.HasValue)
        {
            return Redirect("/public");
        }

       var sqlQuery = @"
            SELECT message.*, user.* 
            FROM message 
            JOIN user ON message.author_id = user.user_id 
            WHERE message.flagged = 0 
            AND (user.user_id = @UserId OR user.user_id IN (
                SELECT whom_id 
                FROM follower
                WHERE who_id = @UserId
            ))
            ORDER BY message.pub_date DESC 
            LIMIT @Limit";

        var parameters = new Dictionary<string, object>
        {
            { "@UserId", userId },
            { "@Limit", 10 } // Assuming you want to limit to 10 posts
        };

        var queryResult = await _databaseService.QueryDb<dynamic>(sqlQuery, parameters);

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

        var viewModel = new UserTimelineViewModel
        {
            currentUserId = userId,
            messages = messages,
        };

        return View("Timeline", viewModel);
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

        var dict = new Dictionary<string, object> { { "@PerPage", _perPage } };
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

    var viewModel = new UserTimelineViewModel
        {
            messages = messages,
        };

        return View("Timeline", viewModel);
    }
    

    /// <summary>
    /// Registers a new message for the user.
    /// </summary>
    [HttpPost("/add_message")]
    public async Task<IActionResult> AddMessage ([FromForm] MessageModel model)
    {
         if (string.IsNullOrEmpty(HttpContext.Session.GetString("user_id"))) {
            return Unauthorized(); 
        } 
        if (!string.IsNullOrEmpty(model.Text)){
            var query = @"INSERT INTO message (author_id, text, pub_date, flagged)  
                            VALUES (@Author_id, @Text, @Pub_date, @Flagged)";
        
        var parameters = new Dictionary<string, object> {
            {"@Author_id", HttpContext.Session.GetString("user_id")!}, 
            {"@Text", Request.Form["text"]},
            {"@Pub_date", (int)DateTimeOffset.Now.ToUnixTimeSeconds()}, 
            {"@Flagged", 0}
        };
        await _databaseService.QueryDb<dynamic>(query, parameters);
        }
        return Redirect("/");
    }




    /// <summary>
    /// Logs the user in.
    /// </summary>
    [HttpGet("/login"), HttpPost("/login")]
    public async Task<IActionResult> Login([FromForm] LoginViewModel model)
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("user_id")))
        {
            return Redirect("/");
        }
        string error = "";
        if (Request.Method == "POST" && ModelState.IsValid)
        {
            var query = "SELECT * FROM user WHERE username = @Username";
            var dict = new Dictionary<string, object> {{"@Username", model.Username!}};
            var users = await _databaseService.QueryDb<UserModel>(query, dict);
            var user = users.FirstOrDefault();

            if (!users.Any())
            {
                error = "Invalid username";
            } else if (model.Password?.GetHashCode().ToString() != user?.pw_hash) {
                error = "Invalid password";
            }
            else
            {
                Console.WriteLine("User logged in with id: " + user.user_id + " and username: " + user.username);
                HttpContext.Session.SetInt32("user_id", user.user_id);
                HttpContext.Session.SetString("username", user.username);
                return Redirect("/");
            }
        }
        if (!string.IsNullOrEmpty(error))
        {
            ModelState.AddModelError("", error); // Add error to entire form
        }
        return View("login");
    }

    /// <summary>
    /// Logs the user out.
    /// </summary>
    /// <returns></returns>
    [HttpGet("/logout")]
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
        var parameters = new Dictionary<string, object> { { "@Username", username } };
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

    [HttpGet("/{username}")]
    public async Task<IActionResult> UserTimeline(string username)
    {

        // Query for the profile user
        var query = "SELECT * FROM user WHERE username = @Username";
        var dict = new Dictionary<string, object> { { "@Username", username } };
        var users = await _databaseService.QueryDb<UserModel>(query, dict);

        UserModel? profileUser = users.FirstOrDefault();

        bool followed = false;

        if (profileUser == null)
        {
            return NotFound();
        }

        var currentUserId = HttpContext.Session.GetInt32("user_id");

        Console.WriteLine("Current user id: " + currentUserId);

        if (currentUserId.HasValue)
        {// Check if the current user is following the profile user
            var followCheckQuery = "SELECT 1 FROM follower WHERE who_id = @CurrentUserId AND whom_id = @ProfileUserId";
            var followCheckParams = new Dictionary<string, object>
        {
            {"CurrentUserId", currentUserId},
            {"ProfileUserId", profileUser.user_id}
        };

            var followCheck = await _databaseService.QueryDb<dynamic>(followCheckQuery, followCheckParams);
            followed = followCheck.Any();
        }

        // Get messages for the user
        var messagesQuery = @"
            SELECT message.*, user.* FROM message
            JOIN user ON user.user_id = message.author_id
            WHERE user.user_id = @UserId
            ORDER BY message.pub_date DESC LIMIT @Limit";
        var queryResult = await _databaseService.QueryDb<dynamic>(messagesQuery, new Dictionary<string, object>
        {
            {"UserId", profileUser.user_id},
            {"Limit", 50} // Assuming PER_PAGE is 50, replace with your actual constant
        });
        // needs to be converted to a new method because it is used twice
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


        var viewModel = new UserTimelineViewModel
        {
            currentUserId = currentUserId,
            profileUser = profileUser,
            messages = messages.ToList(),
            followed = followed
        };

        return View("Timeline", viewModel);
    }

}