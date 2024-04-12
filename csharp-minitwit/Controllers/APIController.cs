using csharp_minitwit.Models;
using csharp_minitwit.Models.DTOs;
using csharp_minitwit.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using System;
using System.Threading.Tasks;

namespace csharp_minitwit.Controllers;

[Route("api")]
[ApiController]

public class ApiController(
    IMessageRepository messageRepository,
    IFollowerRepository followerRepository,
    IUserRepository userRepository,
    IMetaDataRepository metaDataRepository,
    IConfiguration configuration,
    ILogger<ApiController> logger)
    : ControllerBase
{
    private readonly int _perPage = configuration.GetValue<int>("Constants:PerPage");
    private readonly ILogger<ApiController> _logger = logger;

    protected bool NotReqFromSimulator(HttpRequest request)
    {
        var isAuthorized = request.Headers.TryGetValue("Authorization", out var fromSimulator) && fromSimulator.ToString() == "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh";
        if (!isAuthorized)
        {
            _logger.LogWarning("Unauthorized request from {IP}", request.HttpContext.Connection.RemoteIpAddress);
        }
        return isAuthorized;
    }

    protected async Task<int?> GetUserIdAsync(string username)
    {
        return await userRepository.GetUserIdAsync(username);
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest()
    {
        var latest = await metaDataRepository.GetLatestAsync();
        return Ok(new { latest });
    }

    protected async Task UpdateLatest(int? latest)
    {
        if (latest.HasValue)
        {
            await metaDataRepository.SetLatestAsync(latest.Value);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] APIRegisterModel model, int? latest)
    {
        _logger.LogInformation("Registering a new user: {Username}", model.username);
        await UpdateLatest(latest);

        if (ModelState.IsValid)
        {
            //Validate form inputs
            //Todo: This can be done in the APIRegisterModel
            if (string.IsNullOrEmpty(model.username))
            {
                ModelState.AddModelError("Username", "You have to enter a username");
            }
            else if (string.IsNullOrEmpty(model.pwd))
            {
                ModelState.AddModelError("Password", "You have to enter a password");
            }
            else if (string.IsNullOrEmpty(model.email))
            {
                ModelState.AddModelError("Email", "You have to enter a valid email address");
            }
            else if (await userRepository.UserExists(model.username))
            {
                ModelState.AddModelError("Username", "The username is already taken");
            }
            else
            {
                var result = await userRepository.InsertUser(model.username, model.email, model.pwd);
                if(result) 
                {
                    _logger.LogInformation("Successfully registered a new user: {Username}", model.username);
                    return NoContent();
                }
                else
                {
                    _logger.LogError("Failed to register a new user: {Username}", model.username);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
        }
        var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        _logger.LogWarning("User registration failed for {Username}. Errors: {ModelErrors}", model.username,  string.Join(", ", modelErrors));  
        return BadRequest(ModelState);
    }

    [HttpGet("msgs")]
    public async Task<IActionResult> Messages(int no, int? latest)
    {
        if (latest != null)
        {
            await UpdateLatest(latest);
        }

        // Check if request is from simulator
        var notFromSimResponse = NotReqFromSimulator(Request);
        if (!notFromSimResponse){
            _logger.LogWarning("Unauthorized request from {IP}", Request.HttpContext.Connection.RemoteIpAddress);
            return Unauthorized();
        }
        int messagesToFetch = no > 0 ? no : _perPage;

        var filteredMsgs = await messageRepository.GetApiMessagesAsync(messagesToFetch);

        _logger.LogInformation("Successfully retrieved messages");
        return Ok(filteredMsgs);
    }

    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessagesPerUser(string username, [FromBody] APIMessageModel model, int? latest)
    {
        _logger.LogInformation("Posting a new message for user {Username}", username);
        if (latest != null)
        {
            await UpdateLatest(latest);
        }

        // Check if request is from simulator
        var notFromSimResponse = NotReqFromSimulator(Request);
        if (!notFromSimResponse){
            _logger.LogWarning("Unauthorized request from {IP}", Request.HttpContext.Connection.RemoteIpAddress);
            return Unauthorized();
        }
        if (!string.IsNullOrEmpty(model.content))
        {
            var userId = await GetUserIdAsync(username);
            if (!userId.HasValue)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return BadRequest("Invalid username.");
            }

            await messageRepository.AddMessageAsync(model.content, userId.Value);

            _logger.LogInformation("Successfully posted a new message for user {Username}", username);
            return NoContent();
        }
        _logger.LogWarning("Content cannot be empty for user {Username}", username);
        return BadRequest("Content cannot be empty.");
    }

    [HttpGet("msgs/{username}")]
    public async Task<IActionResult> GetMessagesPerUser(string username, int no, int? latest)
    {
        _logger.LogInformation("Retrieving messages for user {Username}", username);
        try
        {
            if (latest != null)
            {
                await UpdateLatest(latest);
            }

            // Check if request is from simulator
            var notFromSimResponse = NotReqFromSimulator(Request);
            if (!notFromSimResponse){
                _logger.LogWarning("Unauthorized request from {IP}", Request.HttpContext.Connection.RemoteIpAddress);
                return Unauthorized();
            }
            var userId = await GetUserIdAsync(username);
            if (!userId.HasValue)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return NotFound("User not found.");
            }

            int messagesToFetch = no > 0 ? no : _perPage;

            var messages = await messageRepository.GetApiMessagesByAuthorAsync(messagesToFetch, userId.Value);

        _logger.LogInformation("Successfully retrieved messages for user {Username}", username);
        return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve messages for user {Username}", username);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve messages");
        }
    }


    [HttpGet("fllws/{username}")]
    public async Task<IActionResult> GetUserFollowers(string username, int no, int? latest)
    {
        _logger.LogInformation("Retrieving followers for user {Username}", username);
        try
        {
            if (latest != null)
            {
                await UpdateLatest(latest);
            }

            // Check if request is from simulator
            var notFromSimResponse = NotReqFromSimulator(Request);
            if (!notFromSimResponse){
                _logger.LogWarning("Unauthorized request from {IP}", Request.HttpContext.Connection.RemoteIpAddress);
                return Unauthorized();
            }
            var userId = await GetUserIdAsync(username);
            if (!userId.HasValue)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return NotFound("User not found.");
            }

            int messagesToFetch = no > 0 ? no : _perPage;

            var followerNames = await followerRepository.GetFollowingNames(messagesToFetch, userId.Value);

            var followersResponse = new
            {
                follows = followerNames
            };

            _logger.LogInformation("Successfully retrieved followers for user {Username}", username);
            return Ok(followersResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve followers for user {Username}", username);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve followers");
        }
    }

    [HttpPost("fllws/{username}")]
    public async Task<IActionResult> FollowUser(string username, [FromBody] FollowActionDto followAction, int? latest)
    {
        
        if (latest != null)
        {
            await UpdateLatest(latest);
        }

        // Check if request is from simulator
        var notFromSimResponse = NotReqFromSimulator(Request);
        if (!notFromSimResponse){
            _logger.LogWarning("Unauthorized request from {IP}", Request.HttpContext.Connection.RemoteIpAddress);
            return Unauthorized();
        }

        var userId = await GetUserIdAsync(username);
        if (!userId.HasValue)
        {
            _logger.LogWarning("User not found: {Username}", username);
            return NotFound("User not found.");
        }

        // Follow
        if (!string.IsNullOrEmpty(followAction.Follow))
        {
            var followsUserId = await GetUserIdAsync(followAction.Follow);
            if (!followsUserId.HasValue)
            {
                _logger.LogWarning("User not found: {Username}", followAction.Follow);
                return NotFound($"User '{followAction.Follow}' not found.");
            }

            await followerRepository.Follow(userId.Value, followsUserId.Value);

            _logger.LogInformation("Successfully followed user {Username}", followAction.Follow);
            return Ok($"Successfully followed user '{followsUserId}'.");
        }

        // Unfollow
        if (!string.IsNullOrEmpty(followAction.Unfollow))
        {
            var followsUserId = await GetUserIdAsync(followAction.Unfollow);
            if (!followsUserId.HasValue)
            {
                _logger.LogWarning("User not found: {Username}", followAction.Unfollow);
                return NotFound($"User '{followAction.Unfollow}' not found.");
            }

            var unfollowed = await followerRepository.Unfollow(userId.Value, followsUserId.Value);

            if (unfollowed)
            {
                _logger.LogInformation("Successfully unfollowed user {Username}", followAction.Unfollow);
                return Ok($"Successfully unfollowed user '{followsUserId}'.");
            }
        }
        _logger.LogWarning("Invalid request for user {Username}", username);
        return BadRequest("Invalid request.");
    }
}