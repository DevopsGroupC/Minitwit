#pragma warning disable S6934 // Specify the RouteAttribute when an HttpMethodAttribute or RouteAttribute is specified at an action level

using System.Diagnostics;

using csharp_minitwit.ActionFilters;
using csharp_minitwit.Models;
using csharp_minitwit.Models.DTOs;
using csharp_minitwit.Models.Entities;
using csharp_minitwit.Models.ViewModels;
using csharp_minitwit.Services.Interfaces;
using csharp_minitwit.Services.Repositories;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Query;

namespace csharp_minitwit.Controllers;

public class HomeController(
    IMessageRepository messageRepository,
    IFollowerRepository followerRepository,
    IUserRepository userRepository,
    IConfiguration configuration,
    ILogger<HomeController> logger)
    : Controller
{
    private readonly int _perPage = configuration.GetValue<int>("Constants:PerPage");
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly ILogger<HomeController> _logger = logger;

    /// <summary>
    /// Shows a users timeline or if no user is logged in it will redirect to the public timeline.
    /// This timeline shows the user's messages as well as all the messages of followed users.
    /// </summary>
    [HttpGet("/")]
    public async Task<IActionResult> Timeline()
    {
        if (TempData["NewlyLoggedIn"] is true)
        {
            ViewBag.newlyLoggedIn = true;
        }

        if (TempData["MessageRecorded"] is true)
        {
            ViewBag.messageRecorded = true;
        }

        var userId = HttpContext.Session.GetInt32("user_id");
        if (!userId.HasValue)
        {
            return Redirect("/public");
        }

        var messages = await messageRepository.GetFollowedMessages(_perPage, userId.Value);

        var viewModel = new UserTimelineViewModel
        {
            CurrentUserId = userId,
            MessagesWithAuthor = messages,
        };

        return View(nameof(Timeline), viewModel);
    }

    [HttpGet("/public")]
    public async Task<IActionResult> PublicTimeline()
    {
        if (TempData["NewlyLoggedOut"] is true)
        {
            ViewBag.newlyLoggedOut = true;
        }

        var messages = await messageRepository.GetMessagesWithAuthorAsync(_perPage);

        var viewModel = new UserTimelineViewModel
        {
            MessagesWithAuthor = messages,
        };

        return View(nameof(Timeline), viewModel);
    }

    [AsyncSessionAuthorize]
    [HttpPost("/add_message")]
    public async Task<IActionResult> AddMessage([FromForm] string text)
    {
        var userId = HttpContext.Session.GetInt32("user_id")!.Value;

        await messageRepository.AddMessageAsync(text, userId);

        TempData["MessageRecorded"] = true;

        _logger.LogInformation("Message added by user {UserId}", userId);
        return Redirect("/");
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromForm] LoginDTO model)
    {

        if (!ModelState.IsValid)
        {
            ViewBag.registrationSuccess = false;
            _logger.LogWarning("Login failed for user: {Username}", model.Username);
            return View(nameof(Login));
        }

        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("user_id")))
        {
            _logger.LogWarning("User already logged in: {Username}", model.Username);
            return Redirect("/");
        }

        var error = string.Empty;
        if (ModelState.IsValid)
        {
            var user = await userRepository.GetByUsername(model.Username);

            if (user == null)
            {
                _logger.LogWarning("Invalid username: {Username}", model.Username);
                error = "Invalid username";
            }
            else if (model.Password == null
            || _passwordHasher.VerifyHashedPassword(user, user.PwHash, model.Password) == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Invalid password for user: {Username}", model.Username);
                error = "Invalid password";
            }
            else
            {
                HttpContext.Session.SetInt32("user_id", user.UserId);
                HttpContext.Session.SetString("username", user.Username);
                TempData["NewlyLoggedIn"] = true;
                _logger.LogInformation("User {Username} logged in", model.Username);
                return RedirectToAction(nameof(Timeline));
            }
        }

        if (!string.IsNullOrEmpty(error))
        {
            ModelState.AddModelError("", error); // Add error to entire form
        }

        _logger.LogWarning("Login failed for user: {Username}", model.Username);
        return View(nameof(Login));
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
        return View(nameof(Login));
    }

    [HttpGet("/logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["NewlyLoggedOut"] = true;
        _logger.LogInformation("User logged out");
        return RedirectToAction(nameof(PublicTimeline));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromForm] RegisterDTO model)
    {
        if (await userRepository.UserExists(model.Username))
        {
            _logger.LogWarning("Username already taken: {Username}", model.Username);
            ModelState.AddModelError("Username", "The username is already taken");
        }

        if (ModelState.IsValid)
        {
            if (await userRepository.InsertUser(model.Username, model.Email, model.Password))
            {
                _logger.LogInformation("User {Username} registered", model.Username);
                return RedirectToAction(nameof(Login), new { registrationSuccess = true });
            }
            _logger.LogError("Failed to register user: {Username}", model.Username);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        _logger.LogWarning("Failed to register user: {Username}", model.Username);
        return View(model);
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpGet("/{username}/followers/{follower}")]


    [HttpGet("/{username}")]
    public async Task<IActionResult> UserTimeline(string username)
    {
        if (TempData[nameof(Message)] is not null)
        {
            ViewBag.message = $"You are no longer following \"{username}\"";
        }

        var profileUser = await userRepository.GetByUsername(username);

        if (profileUser == null)
        {
            return NotFound();
        }

        var currentUserId = HttpContext.Session.GetInt32("user_id");
        var followed = false;

        if (currentUserId.HasValue) // Check if the current user is following the profile user
        {
            followed = await followerRepository.IsFollowing(currentUserId.Value, profileUser.UserId);
        }

        var messages = await messageRepository.GetMessagesByAuthorAsync(_perPage, profileUser.UserId);

        var viewModel = new UserTimelineViewModel
        {
            CurrentUserId = currentUserId,
            ProfileUser = profileUser,
            MessagesWithAuthor = messages,
            Followed = followed
        };

        return View(nameof(Timeline), viewModel);
    }

    [AsyncSessionAuthorize]
    [HttpGet("{username}/follow")]
    public async Task<IActionResult> FollowUser(string username)
    {
        var currentUserId = HttpContext.Session.GetInt32("user_id")!.Value;
        var profileUser = await userRepository.GetByUsername(username);

        if (profileUser == null)
        {
            return NotFound();
        }

        await followerRepository.Follow(currentUserId, profileUser.UserId);

        TempData[nameof(Message)] = $"You are now following {username}";

        return RedirectToAction(nameof(UserTimeline), new { username });
    }

    [AsyncSessionAuthorize]
    [HttpGet("{username}/unfollow")]
    public async Task<IActionResult> UnfollowUser(string username)
    {
        var currentUserId = HttpContext.Session.GetInt32("user_id")!.Value;

        var profileUser = await userRepository.GetByUsername(username);
        if (profileUser == null)
        {
            return NotFound();
        }

        await followerRepository.Unfollow(currentUserId, profileUser.UserId);


        TempData[nameof(Message)] = $"You are no longer following {username}";
        return RedirectToAction(nameof(UserTimeline), new { username });
    }
}
#pragma warning restore S6934 //enabeling the warning again