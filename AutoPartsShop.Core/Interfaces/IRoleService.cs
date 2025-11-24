using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPartsShop.Domain.Dtos;

namespace AutoPartsShop.Core.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetRolesAsync();
    Task<(bool Success, RoleDto? Data, string? Error)> CreateRoleAsync(string name);
    Task<(bool Success, string? Error)> DeleteRoleAsync(string name);
}
