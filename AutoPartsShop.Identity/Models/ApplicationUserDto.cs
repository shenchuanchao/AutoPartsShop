using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsShop.Identity.Models;

public class ApplicationUserDto
{
    public string? Id { get; set; }

    public string? Email { get; set; }

    [Display(Name = "Email Confirmed")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Phone Number Confirmed")]
    public bool PhoneNumberConfirmed { get; set; }

    [Display(Name = "Full Name")]
    public string? FullName { get; set; }

    [Display(Name = "User Name")]
    public string? UserName { get; set; }
    public string? Title { get; set; }

    [Display(Name = "Company Name")]
    public string? CompanyName { get; set; }

    public string? Photo { get; set; }
}
