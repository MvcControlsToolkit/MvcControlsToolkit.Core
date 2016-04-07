using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Types;

namespace MvcControlsToolkit.Core.ITests.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public float? AFloat { get; set; }
        public DateTime ADatetime { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ADate { get; set; }

        [DataType(DataType.Time)]
        public DateTime? ATime { get; set; }

        [DataType("Week")]
        public Week? AWeek { get; set; }

        [DataType("Month")]
        public Month? AMonth { get; set; }

    }
}
