using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace AutoPartsShop.Infrastructure.Services;

public class RoleService: IRoleService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoleService> _logger;

    public RoleService(AppDbContext context, ILogger<RoleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Implement IRoleService methods here
    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<Domain.Dtos.RoleDto>> GetRolesAsync()
    {
        try
        {
            var roles = _context.Roles.Select(r => new Domain.Dtos.RoleDto
            {
                Id = r.Id,
                Name = r.Name??"",
                NormalizedName = r.NormalizedName
            }).ToList();

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return new List<Domain.Dtos.RoleDto>();
        }
    }
    /// <summary>
    /// 添加一个角色
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<(bool Success, Domain.Dtos.RoleDto? Data, string? Error)> CreateRoleAsync(string name)
    {
        try
        {
            var role = new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Name = name,
                NormalizedName = name.ToUpper()
            };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            var roleDto = new Domain.Dtos.RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                NormalizedName = role.NormalizedName
            };
            return (true, roleDto, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return (false, null, ex.Message);
        }
    }
    /// <summary>
    /// 删除一个角色
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<(bool Success, string? Error)> DeleteRoleAsync(string name)
    {
        try
        {
            var role = _context.Roles.FirstOrDefault(r => r.Name == name);
            if (role == null)
            {
                return (false, "Role not found");
            }
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role");
            return (false, ex.Message);
        }
    }


}
