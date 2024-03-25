using csharp_minitwit.Models;
using csharp_minitwit.Models.DTOs;
using csharp_minitwit.Services.Interfaces;
using csharp_minitwit.Utils;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace csharp_minitwit.Controllers;


[Route("api")]
[ApiController]
public class ApiController(
    IMessageRepository messageRepository,
    IFollowerRepository followerRepository,
    IUserRepository userRepository,
    IConfiguration configuration)
    : ControllerBase
{
    private readonly string _perPage;

    protected bool NotReqFromSimulator(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Authorization", out var fromSimulator))
        {
            if (fromSimulator.ToString() == "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
            {
                return true;
            }
        }
        return false;
    }

    protected async Task<int?> GetUserIdAsync(string username)
    {
        return await userRepository.GetUserIdAsync(username);
    }

    [HttpGet("latest")]
    public IActionResult GetLatest()
    {
        int latestProcessedCommandID;
        try
        {
            var latest = System.IO.File.ReadAllText("Services/latest_processed_sim_action_id.txt");
            latestProcessedCommandID = int.Parse(latest);
        }
        catch (Exception)
        {
            latestProcessedCommandID = -1;
        }
        return Ok(new { latest = latestProcessedCommandID });
    }

    protected void UpdateLatest(int? latest)
    {

        System.IO.File.WriteAllText("Services/latest_processed_sim_action_id.txt", latest.ToString());

    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] APIRegisterModel model, int? latest)
    {

        UpdateLatest(latest);

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
            else if (await userRepository.InsertUser(model.username, model.email, model.pwd))
            {
                return NoContent();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        return BadRequest(ModelState);
    }

    [HttpGet("msgs")]
    public async Task<IActionResult> Messages(int no, int? latest)
    {
        if (latest != null)
        {
            UpdateLatest(latest);
        }

        // Check if request is from simulator
        var notFromSimResponse = NotReqFromSimulator(Request);
        if (!notFromSimResponse)
            return Unauthorized();

        int messagesToFetch = no > 0 ? no : int.Parse(_perPage);

        var filteredMsgs = await messageRepository.GetApiMessagesAsync(messagesToFetch);

        return Ok(filteredMsgs);
    }

    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessagesPerUser(string username, [FromBody] APIMessageModel model, int? latest)
    {
        if (latest != null)
        {
            UpdateLatest(latest);
        }

        // Check if request is from simulator
        var notFromSimResponse = NotReqFromSimulator(Request);
        if (!notFromSimResponse)
            return Unauthorized();

        if (!string.IsNullOrEmpty(model.content))
        {
            var userId = await GetUserIdAsync(username);
            if (!userId.HasValue)
            {
                return BadRequest("Invalid username.");
            }

            await messageRepository.AddMessageAsync(model.content, userId.Value);

            return NoContent();
        }

        return BadRequest("Content cannot be empty.");
    }

    [HttpGet("msgs/{username}")]
    public async Task<IActionResult> GetMessagesPerUser(string username, int no, int? latest)
    {
        if (latest != null)
        {
            UpdateLatest(latest);
        }

        // Check if request is from simulator
        var notFromSimResponse = NotReqFromSimulator(Request);
        if (!notFromSimResponse)
            return Unauthorized();

        var userId = await GetUserIdAsync(username);
        if (!userId.HasValue)
        {
            return NotFound("User not found.");
        }

        int messagesToFetch = no > 0 ? no : int.Parse(_perPage);

        var messages = await messageRepository.GetApiMessagesByAuthorAsync(messagesToFetch, userId.Value);

        return Ok(messages);
    }


    [HttpGet("fllws/{username}")]
    public async Task<IActionResult> GetUserFollowers(string username, int no, int? latest)
    {
        if (latest != null)
        {
            UpdateLatest(latest);
        }

        // Check if request is from simulator
        var notFromSimResponse = NotReqFromSimulator(Request);
        if (!notFromSimResponse)
            return Unauthorized();

        var userId = await GetUserIdAsync(username);
        if (!userId.HasValue)
        {
            return NotFound("User not found.");
        }

        int messagesToFetch = no > 0 ? no : int.Parse(_perPage);

        var followerNames = await followerRepository.GetFollowingNames(messagesToFetch, userId.Value);

        var followersResponse = new
        {
            follows = followerNames
        };

        return Ok(followersResponse);
    }

    [HttpPost("fllws/{username}")]
    public async Task<IActionResult> FollowUser(string username, [FromBody] FollowActionDto followAction, int? latest)
    {
        if (latest != null)
        {
            UpdateLatest(latest);
        }

        // Check if request is from simulator
        var notFromSimResponse = NotReqFromSimulator(Request);
        if (!notFromSimResponse)
            return Unauthorized();

        var userId = await GetUserIdAsync(username);
        if (!userId.HasValue)
        {
            return NotFound("User not found.");
        }

        // Follow
        if (!string.IsNullOrEmpty(followAction.Follow))
        {
            var followsUserId = await GetUserIdAsync(followAction.Follow);
            if (!followsUserId.HasValue)
            {
                return NotFound($"User '{followAction.Follow}' not found.");
            }

            await followerRepository.Follow(userId.Value, followsUserId.Value);

            return Ok($"Successfully followed user '{followsUserId}'.");
        }

        // Unfollow
        if (!string.IsNullOrEmpty(followAction.Unfollow))
        {
            var followsUserId = await GetUserIdAsync(followAction.Unfollow);
            if (!followsUserId.HasValue)
            {
                return NotFound($"User '{followAction.Unfollow}' not found.");
            }

            var unfollowed = await followerRepository.Unfollow(userId.Value, followsUserId.Value);

            if (unfollowed)
            {
                return Ok($"Successfully unfollowed user '{followsUserId}'.");
            }
        }
        return BadRequest("Invalid request.");
    }
}