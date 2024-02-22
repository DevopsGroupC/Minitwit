using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using csharp_minitwit.Models;
using csharp_minitwit.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;

namespace csharp_minitwit.Controllers;


[Route("api")]
[ApiController]
public class APIController : ControllerBase
{

    private readonly IDatabaseService _databaseService;
    private readonly IConfiguration _configuration;
    private readonly string _perPage;
    private readonly PasswordHasher<UserModel> _passwordHasher;

    public APIController(IDatabaseService databaseService, IConfiguration configuration)
    {
        _databaseService = databaseService;
        _configuration = configuration;
        _perPage = configuration.GetValue<string>("Constants:PerPage")!;
        _passwordHasher = new PasswordHasher<UserModel>();
    }



    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("Test");
    }

    public string NotReqFromSimulator(HttpRequest request)
    {
        var fromSimulator = request.Headers["Authorization"].ToString();
        Console.WriteLine(fromSimulator);
        if (fromSimulator != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmU=")
        {
            var error = "You are not authorized to use this resource!";
            return error;
        }
        return null;
    }

    [HttpGet("latest")]
    public IActionResult GetLatest()
    {
        var latestProcessedCommandID = 0;
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

    public void updateLatest(string latest)
    {
        int parsedLatest = -1;
        try
        {
            parsedLatest = int.Parse(latest);
        }
        catch (System.Exception)
        {
        }
        if (parsedLatest != -1)
        {
            System.IO.File.WriteAllText("Services/latest_processed_sim_action_id.txt", parsedLatest.ToString());
        }
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// 
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] APIRegisterModel model, string latest)
    {

        updateLatest(latest);

        if (ModelState.IsValid)
        {
            //Validate form inputs
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
            else if (await IsUsernameTaken(model.username))
            {
                ModelState.AddModelError("Username", "The username is already taken");
            }
            else
            {
                //Insert user into database
                var result = await InsertUser(model.username, model.email, model.pwd);
                //TempData["SuccessMessage"] = "You were successfully registered and can login now"; //Not needed? for the frontEnd?
                return NoContent();
            }
        }
        return BadRequest(ModelState);
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



    /// <summary>
    /// Registers a new message for the user.
    /// </summary>
    [HttpGet("msgs")]
    public async Task<IActionResult> Messages(string latest)
    {
        // Update latest
        updateLatest(latest);

        // Check if request is from simulator
        var notFromSimResponse = NotReqFromSimulator(Request);
        if (notFromSimResponse != null)
             return Forbid(notFromSimResponse);

        var sqlQuery = @"
            SELECT message.*, user.*
            FROM message
            INNER JOIN user ON message.author_id = user.user_id
            WHERE message.flagged = 0
            ORDER BY message.pub_date DESC
            LIMIT @PerPage";

        var dict = new Dictionary<string, object> { { "@PerPage", _perPage } };
        var queryResult = await _databaseService.QueryDb<dynamic>(sqlQuery, dict);

        Console.WriteLine("queryres" + queryResult.Count());
        var filteredMsgs = queryResult.Select(msg =>
        {
            var dictB = (IDictionary<string, object>)msg;
            return new APIMessageModel
            {
                content = (string)dictB["text"],
                pub_date = (long)dictB["pub_date"],
                user = (string)dictB["username"]
            };
        }).ToList();
        Console.WriteLine(filteredMsgs);
        return Ok(filteredMsgs);
    }


}

        //     else if (Request.Method == "POST")
        // {
        //  if (string.IsNullOrEmpty(HttpContext.Session.GetString("username"))) {
        //     return Unauthorized(); 
        // } 
        // if (!string.IsNullOrEmpty(model.Text)){
        //     var query = @"INSERT INTO message (author_id, text, pub_date, flagged)  
        //                     VALUES (@Author_id, @Text, @Pub_date, @Flagged)";
        
        // var parameters = new Dictionary<string, object> {
        //     {"@Author_id", HttpContext.Session.GetInt32("user_id")}, 
        //     {"@Text", model.Text},
        //     {"@Pub_date", (long)DateTimeOffset.Now.ToUnixTimeSeconds()}, 
        //     {"@Flagged", 0}
        // };
        // await _databaseService.QueryDb<dynamic>(query, parameters);
        // }
        // return NoContent();
        // }

        // return BadRequest();
