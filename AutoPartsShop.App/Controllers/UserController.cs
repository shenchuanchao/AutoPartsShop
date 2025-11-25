using AutoPartsShop.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using AutoPartsShop.Domain.Dtos;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using AutoPartsShop.Infrastructure.Data;

namespace AutoPartsShop.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class UserController(UserManager<ApplicationUser> userManager, ILogger<UserController> logger) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<UserController> _logger = logger;

    [HttpGet("")]
    [EnableQuery]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IQueryable<ApplicationUserDto>> Get()
    {
        var users = _userManager.Users.Select(x => new ApplicationUserDto
        {
            Id = x.Id,
            Email = x.Email,
            EmailConfirmed = x.EmailConfirmed,
            PhoneNumber = x.PhoneNumber,
            PhoneNumberConfirmed = x.PhoneNumberConfirmed,
            FullName = x.FullName,
            UserName = x.UserName,
            CompanyName=x.CompanyName,
            Photo=x.Photo
        });

        foreach (var user in users)
        {
            _logger.LogInformation("User Info: Id={Id}, Email={Email}, UserName={UserName}, FullName={FullName}", user.Id, user.Email, user.UserName, user.FullName);
        }

        return Ok(users);
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationUserWithRolesDto>> GetAsync(string key)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (key != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(key);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new ApplicationUserWithRolesDto
        {
            Id = user.Id,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            UserName = user.UserName,
            FullName = user.FullName,
            CompanyName = user.CompanyName,
            Photo = user.Photo,
            Roles = [.. (await _userManager.GetRolesAsync(user))],
        });
    }

    [HttpGet("@me")]
    [ODataIgnored]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationUserWithRolesDto>> GetMeAsync()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return NotFound();
        }

        return await GetAsync(userId);
    }

    [HttpPut("{key}")]
    [ODataIgnored]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationUserDto>> PutAsync(string key, UpdateUserRequest update)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (key != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(key);

        if (user == null)
        {
            return NotFound();
        }

        user.UserName = update.UserName;
        user.FullName = update.FullName;
        user.PhoneNumber = update.PhoneNumber;
        user.CompanyName = update.CompanyName;
        user.Photo = update.Photo;

        await _userManager.UpdateAsync(user);

        return Ok(new ApplicationUserDto
        {
            Id = user.Id,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            UserName = user.UserName,
            FullName = user.FullName,
        });
    }

    [HttpPut("@me")]
    [ODataIgnored]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationUserDto>> PutMeAsync(UpdateUserRequest update)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return NotFound();
        }

        return await PutAsync(userId, update);
    }

    [HttpDelete("{key}")]
    [ODataIgnored]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(string key)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (key != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(key);

        if (user == null)
        {
            return NotFound();
        }

        if ((await _userManager.GetUsersInRoleAsync("Admin")).Count == 1
            && await _userManager.IsInRoleAsync(user, "Admin"))
        {
            ModelStateDictionary errors = new();
            errors.AddModelError("LastAdmin", "The last Admin cannot be deleted.");

            return BadRequest(errors);
        }

        await _userManager.DeleteAsync(user);

        return NoContent();
    }

    [HttpDelete("@me")]
    [ODataIgnored]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMeAsync()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return NotFound();
        }

        return await DeleteAsync(userId);
    }

    [HttpPut("{key}/roles")]
    [Authorize(Roles = "Admin")]
    [ODataIgnored]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutRolesAsync(string key, IEnumerable<string> roles)
    {
        var user = await _userManager.FindByIdAsync(key);

        if (user == null)
        {
            return NotFound();
        }

        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin") ?? [];

        var userRoles = await _userManager.GetRolesAsync(user) ?? [];

        var removeRoles = userRoles.Where(x => !roles.Contains(x));
        var addRoles = roles.Where(x => !userRoles.Contains(x));

        if (removeRoles.Contains("Admin") && !adminUsers.Any(x => x.Id != key))
        {
            ModelStateDictionary errors = new();
            errors.AddModelError("LastAdmin", "The last Admin cannot be removed from the Admin role.");

            return BadRequest(errors);
        }

        await _userManager.RemoveFromRolesAsync(user, removeRoles);
        await _userManager.AddToRolesAsync(user, addRoles);

        return NoContent();
    }
}
