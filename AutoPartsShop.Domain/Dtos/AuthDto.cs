using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPartsShop.Domain.Models;

namespace AutoPartsShop.Domain.Dtos
{
    /// <summary>
    /// 注册请求DTO
    /// </summary>
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Photo { get; set; }
    }

    /// <summary>
    /// 登录请求DTO
    /// </summary>
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true;

    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public User user { get; set; } = new();
    }


}
