using System.Security.Claims;
using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Domain.Dtos;
using AutoPartsShop.Identity.Models;
using AutoPartsShop.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace AutoPartsShop.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RoleController> _logger;

    public RoleController(RoleManager<IdentityRole> roleManager, ILogger<RoleController> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有角色
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [EnableQuery]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IQueryable<RoleDto>> Get()
    {
        var roles = _roleManager.Roles.Select(x => new RoleDto
        {
            Id = x.Id,
            Name = x.Name ?? "",
            NormalizedName = x.NormalizedName
        });

        return Ok(roles);

    }

    [HttpGet("select")]
    [ODataIgnored]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<List<RoleDto>>> GetSelectAsync()
    {
        var roles = _roleManager.Roles.Select(x => new RoleDto
        {
            Id = x.Id,
            Name = x.Name ?? ""
        }).ToList();
        return Task.FromResult<ActionResult<List<RoleDto>>>(Ok(roles));
    }


}
