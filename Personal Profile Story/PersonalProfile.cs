using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ManagementPortal.Models
{
    public class PersonalProfile
    {

        [Key]
        [ForeignKey("Person")]
        public string ProfileID { get; set; }

        [Required]
        [MaxLength(100)]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "About Me must be between 1 and 100 characters")]
        [Display(Name = "AboutMe")]
        public string AboutMe { get; set; }


        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Tagline must be between 1 and 100 characters")]
        [Display(Name = "Tagline")]
        public string TagLine { get; set; }

        public virtual ApplicationUser Person { get; set; }

        public static implicit operator PersonalProfile(string v)
        {
            throw new NotImplementedException();
        }
    }
}