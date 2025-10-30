using AutoPartsShop.Domain.Dtos;
using AutoPartsShop.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsShop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles ="Admin")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<ApplicationUser> userManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }
        /// <summary>
        /// 获取用户列表（分页）
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserQueryRequest request)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();

                // 关键字查询（Email、FirstName、LastName）
                if (!string.IsNullOrWhiteSpace(request.Keyword))
                {
                    var keyword = request.Keyword.Trim().ToLower();
                    query = query.Where(u =>
                        u.Email.ToLower().Contains(keyword) ||
                        u.UserName.ToLower().Contains(keyword) ||
                        u.FullName.ToLower().Contains(keyword));
                }

                // 电话号码查询
                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    var phoneNumber = request.PhoneNumber.Trim();
                    query = query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(phoneNumber));
                }

                // 邮箱确认状态筛选
                if (request.EmailConfirmed.HasValue)
                {
                    query = query.Where(u => u.EmailConfirmed == request.EmailConfirmed.Value);
                }

                // 创建时间范围筛选
                if (request.CreatedFrom.HasValue)
                {
                    query = query.Where(u => u.CreatedAt >= request.CreatedFrom.Value);
                }
                if (request.CreatedTo.HasValue)
                {
                    query = query.Where(u => u.CreatedAt <= request.CreatedTo.Value);
                }

                // 排序
                query = request.SortBy?.ToLower() switch
                {
                    "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                    "name" => request.SortDescending ?
                        query.OrderByDescending(u => u.FullName).ThenByDescending(u => u.UserName) :
                        query.OrderBy(u => u.FullName).ThenBy(u => u.UserName),
                    "createdat" => request.SortDescending ?
                        query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                    _ => request.SortDescending ?
                        query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt) // 默认按创建时间排序
                };

                var totalCount = await query.CountAsync();

                // 分页
                var pageNumber = request.Page ?? 1;
                var pageSize = request.PageSize ?? 10;
                var users = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserResponse
                    {
                        Id = u.Id,
                        Email = u.Email,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        PhoneNumber = u.PhoneNumber,
                        EmailConfirmed = u.EmailConfirmed,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                // 获取每个用户的角色（由于EF Core限制，需要单独查询）
                foreach (var user in users)
                {
                    var userEntity = await _userManager.FindByIdAsync(user.Id);
                    if (userEntity != null)
                    {
                        user.Roles = await _userManager.GetRolesAsync(userEntity);
                    }
                }

                var result = new
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    Users = users,
                    Query = request
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户列表时发生错误");
                return StatusCode(500, new { message = "获取用户列表失败" });
            }
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userResponse = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    Roles = roles
                };

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息时发生错误");
                return StatusCode(500, new { message = "获取用户信息失败" });
            }
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="request">创建用户请求</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // 检查邮箱是否已存在
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "邮箱已被使用" });
                }

                var user = new ApplicationUser
                {
                    Email = request.Email,
                    UserName = request.UserName,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    EmailConfirmed = true, // 管理员创建的用户默认确认邮箱
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                // 分配角色
                if (request.Roles != null && request.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
                    if (!roleResult.Succeeded)
                    {
                        // 如果角色分配失败，删除已创建的用户
                        await _userManager.DeleteAsync(user);
                        return BadRequest(new { errors = roleResult.Errors.Select(e => e.Description) });
                    }
                }

                _logger.LogInformation("管理员创建用户成功: {Email}", request.Email);

                var userResponse = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    Roles = await _userManager.GetRolesAsync(user)
                };

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户时发生错误");
                return StatusCode(500, new { message = "创建用户失败" });
            }
        }


        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="request">更新用户请求</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                // 更新基本信息
                user.UserName = request.UserName ?? user.UserName;
                user.FullName = request.FullName ?? user.FullName;
                user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                // 更新角色
                if (request.Roles != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        return BadRequest(new { errors = removeResult.Errors.Select(e => e.Description) });
                    }

                    var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                    if (!addResult.Succeeded)
                    {
                        return BadRequest(new { errors = addResult.Errors.Select(e => e.Description) });
                    }
                }

                _logger.LogInformation("管理员更新用户成功: {UserId}", id);

                return Ok(new { message = "用户更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户时发生错误");
                return StatusCode(500, new { message = "更新用户失败" });
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                // 防止管理员删除自己
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (user.Id == currentUserId)
                {
                    return BadRequest(new { message = "不能删除自己的账户" });
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                _logger.LogInformation("管理员删除用户成功: {UserId}", id);

                return Ok(new { message = "用户删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户时发生错误");
                return StatusCode(500, new { message = "删除用户失败" });
            }
        }

        /// <summary>
        /// 重置用户密码
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="request">重置密码请求</param>
        /// <returns></returns>
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                // 生成密码重置令牌
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                _logger.LogInformation("管理员重置用户密码成功: {UserId}", id);

                return Ok(new { message = "密码重置成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置密码时发生错误");
                return StatusCode(500, new { message = "重置密码失败" });
            }
        }

    }
}
