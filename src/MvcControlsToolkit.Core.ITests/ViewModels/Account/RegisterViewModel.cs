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
        [DataType(DataType.EmailAddress)]
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

        [Range(10.2, 20.2)]
        public float? AFloat { get; set; }
        public DateTime? ADatetime { get; set; }

        [Range(typeof(DateTime), "2016-01-01", "2016-11-01")]
        [DataType(DataType.Date)]
        public DateTime? ADate { get; set; }

        [DataType(DataType.Time)]
        [Range(typeof(TimeSpan), "10:00:00", "20:00:00")]
        public TimeSpan? ATime { get; set; }

        [DataType("Week")]
        [Range(typeof(Week), "2016-W10", "2016-W30")]
        public Week? AWeek { get; set; }

        [DataType("Month")]
        [Range(typeof(Month), "2016-01", "2016-10")]
        public Month? AMonth { get; set; }

        [DataType("Color", ErrorMessage = "{0} non è un colore")]
        public string AColor { get; set; }

        [DataType(DataType.Url)]
        public string AUrl { get; set; }

        
        public uint APositiveInteger  { get; set; }
    }
}
