using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using csharp_minitwit.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using csharp_minitwit.Models;
using csharp_minitwit.Utils;
using System.Text.RegularExpressions;


namespace csharp_minitwit.Controllers;

public class HomeController : Controller
{
    private readonly IDatabaseService _databaseService;
    private readonly string _perPage;
    private readonly PasswordHasher<UserModel> _passwordHasher;

    public HomeController(IDatabaseService databaseService, IConfiguration configuration)
    {
        _databaseService = databaseService;
        _perPage = configuration.GetValue<string>("Constants:PerPage")!;
        _passwordHasher = new PasswordHasher<UserModel>();
    }
    /// <summary>
    /// Shows a users timeline or if no user is logged in it will redirect to the public timeline.
    /// This timeline shows the user's messages as well as all the messages of followed users.
    /// </summary>
    [HttpGet("/")]
    public async Task<IActionResult> Timeline()
    {
        var newlyLoggedIn = TempData["NewlyLoggedIn"] as bool?;
        if (newlyLoggedIn.HasValue && newlyLoggedIn.Value)
        {
            ViewBag.newlyLoggedIn = true;
        }
        var messageRecorded = TempData["MessageRecorded"] as bool?;
        if (messageRecorded.HasValue && messageRecorded.Value)
        {
            ViewBag.messageRecorded = true;
        }

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

        var messages = MessageHelper.MessageConverter(queryResult);

        var viewModel = new UserTimelineModel
        {
            CurrentUserId = userId,
            Messages = messages,
        };

        return View("Timeline", viewModel);
    }


    /// <summary>
    /// Displays the latest messages of all users.
    /// </summary>
    [HttpGet("/public")]
    public async Task<IActionResult> PublicTimeline()
    {
        var newlyLoggedOut = TempData["NewlyLoggedOut"] as bool?;
        if (newlyLoggedOut.HasValue && newlyLoggedOut.Value)
        {
            ViewBag.newlyLoggedOut = true;
        }
        
        var sqlQuery = @"
            SELECT message.*, user.*
            FROM message
            INNER JOIN user ON message.author_id = user.user_id
            WHERE message.flagged = 0
            ORDER BY message.pub_date DESC
            LIMIT @PerPage";

        var dict = new Dictionary<string, object> { { "@PerPage", _perPage } };
        var queryResult = await _databaseService.QueryDb<dynamic>(sqlQuery, dict);

        var messages = MessageHelper.MessageConverter(queryResult);

        var viewModel = new UserTimelineModel
        {
            Messages = messages,
        };

        return View("Timeline", viewModel);
    }

    /// <summary>
    /// Registers a new message for the user.
    /// </summary>
    [HttpPost("/add_message")]
    public async Task<IActionResult> AddMessage([FromForm] MessageModel model)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("username")))
        {
            return Unauthorized();
        }
        if (!string.IsNullOrEmpty(model.Text))
        {
            var query = @"INSERT INTO message (author_id, text, pub_date, flagged)  
                            VALUES (@Author_id, @Text, @Pub_date, @Flagged)";

            var parameters = new Dictionary<string, object> {
            {"@Author_id", HttpContext.Session.GetInt32("user_id")},
            {"@Text", model.Text},
            {"@Pub_date", (long)DateTimeOffset.Now.ToUnixTimeSeconds()},
            {"@Flagged", 0}
        };
            await _databaseService.QueryDb<dynamic>(query, parameters);
            TempData["MessageRecorded"] = true;
        }
        return Redirect("/");
    }

    /// <summary>
    /// Logs the user in.
    /// </summary>
    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromForm] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.registrationSuccess = false;
            return View("login");
        }

        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("user_id")))
        {
            return Redirect("/");
        }
        string error = "";
        if (Request.Method == "POST" && ModelState.IsValid)
        {
            var query = "SELECT * FROM user WHERE username = @Username";
            var dict = new Dictionary<string, object> { { "@Username", model.Username! } };
            var users = await _databaseService.QueryDb<UserModel>(query, dict);
            var user = users.FirstOrDefault();

            if (user == null)
            {
                error = "Invalid username";
            }
            else if (model.Password == null
            || _passwordHasher.VerifyHashedPassword(user, user.pw_hash, model.Password) == PasswordVerificationResult.Failed)
            {
                error = "Invalid password";
            }
            else
            {
                Console.WriteLine("User logged in with id: " + user.user_id + " and username: " + user.username);
                HttpContext.Session.SetInt32("user_id", user.user_id);
                HttpContext.Session.SetString("username", user.username);
                TempData["NewlyLoggedIn"] = true;
                return RedirectToAction("Timeline");
            }
        }
        if (!string.IsNullOrEmpty(error))
        {
            ModelState.AddModelError("", error); // Add error to entire form
        }
        return View("login");
    }

    [HttpGet("/login")]
    public IActionResult Login(bool? registrationSuccess)
    {
        if (registrationSuccess.HasValue && registrationSuccess.Value)
        {
            ViewBag.registrationSuccess = true;
        }
        else
        {
            ViewBag.registrationSuccess = false;
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
        HttpContext.Session.Clear();
        TempData["NewlyLoggedOut"] = true;
        return RedirectToAction("PublicTimeline");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// 
    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromForm] RegisterModel model)
    {
        if (true)
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
                else if (!IsValidEmailDomain(model.Email))
                {
                    ModelState.AddModelError("Email", "You have to enter a valid email address");
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
                    var result = await InsertUser(model.Username, model.Email, model.Password);
                    return RedirectToAction("Login", new { registrationSuccess = true });
                }
            }
            // If model state is not valid, return back to the registration form with validation errors
            return View(model);
        }
        
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        return View();
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

        var hashedPassword = _passwordHasher.HashPassword(new UserModel(), password);

        var parameters = new Dictionary<string, object>
        {
            { "@Username", username },
            { "@Email", email },
            { "@Password", hashedPassword }
        };
        return await _databaseService.QueryDb<dynamic>(sqlQuery, parameters);
    }


    private bool IsValidEmailDomain(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            return false;
        }
        var domain = email.Split('@')[1];
        var domainPattern = @"^[a-zA-Z]+\.((com)|(dk))$"; 
        return Regex.IsMatch(domain, domainPattern);
    }


    [HttpGet("/{username}")]
    public async Task<IActionResult> UserTimeline(string username)
    {
        var message = TempData["Message"] as bool?;
        if (message.HasValue && message.Value)
        {
            ViewBag.message = true;
        }
        
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
            {"Limit", _perPage}
        });

        var messages = MessageHelper.MessageConverter(queryResult);

        var viewModel = new UserTimelineModel
        {
            CurrentUserId = currentUserId,
            ProfileUser = profileUser,
            Messages = messages.ToList(),
            Followed = followed
        };

        return View("Timeline", viewModel);
    }

    [HttpGet("{username}/follow")]
    public async Task<IActionResult> FollowUser(string username)
    {
        // Query for the profile user
        var query = "SELECT * FROM user WHERE username = @Username";
        var dict = new Dictionary<string, object> { { "@Username", username } };
        var users = await _databaseService.QueryDb<UserModel>(query, dict);

        UserModel? profileUser = users.FirstOrDefault();

        if (profileUser == null)
        {
            return NotFound();
        }


        var currentUserId = HttpContext.Session.GetInt32("user_id");

        var parameters = new Dictionary<string, object>
        {
            { "@WhoId", currentUserId },
            { "@WhomId", profileUser.user_id }
        };

        var sqlQuery = "INSERT INTO follower (who_id, whom_id) VALUES (@WhoId, @WhomId)";

        await _databaseService.QueryDb<int>(sqlQuery, parameters);

        TempData["Message"] = $"You are now following {username}";
        return RedirectToAction("UserTimeline", new { username = username });
    }

    [HttpGet("{username}/unfollow")]
    public async Task<IActionResult> UnfollowUser(string username)
    {
        // Query for the profile user
        var query = "SELECT * FROM user WHERE username = @Username";
        var dict = new Dictionary<string, object> { { "@Username", username } };
        var users = await _databaseService.QueryDb<UserModel>(query, dict);

        UserModel? profileUser = users.FirstOrDefault();

        if (profileUser == null)
        {
            return NotFound();
        }

        var currentUserId = HttpContext.Session.GetInt32("user_id"); //maybe it has to be changed

        var parameters = new Dictionary<string, object>
        {
            { "@WhoId", currentUserId },
            { "@WhomId", profileUser.user_id }
        };

        var sqlQuery = "DELETE FROM follower WHERE who_id = @WhoId AND whom_id = @WhomId";

        await _databaseService.QueryDb<int>(sqlQuery, parameters);

        TempData["Message"] = $"You are no longer following {username}";
        return RedirectToAction("UserTimeline", new { username = username });
    }

}