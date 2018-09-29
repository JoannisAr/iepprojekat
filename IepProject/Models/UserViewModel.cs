using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IepProject0.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public String Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public String Password { get; set; }

    }

    public class RegsisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public String FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public String LastName { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public String Email { get; set; }


        [Required]
        [StringLength(255, ErrorMessage = "Password must be at least 8 characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public String Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Confirmation password does not match with password.")]
        public string ConfirmPassword { get; set; }

    }

    public class ChangePassViewModel
    {

        [Required]
        [StringLength(255, ErrorMessage = "Password must be at least 8 characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Previous password")]
        public String OldPassword { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Password must be at least 8 characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public String NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [StringLength(255, ErrorMessage = "Password must be at least 8 characters long.", MinimumLength = 8)]
        [Compare("NewPassword", ErrorMessage = "Confirmation password does not match with new password.")]
        public string ConfirmNewPassword { get; set; }
    }
 }