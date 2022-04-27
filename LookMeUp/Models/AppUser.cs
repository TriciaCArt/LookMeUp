using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LookMeUp.Models
{
    public class AppUser : IdentityUser 
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "Nope. Try again. the {0} myst be at least {2} and at max {1} characters long", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Nope. Try again. the {0} myst be at least {2} and at max {1} characters long", MinimumLength = 2)]
        public string? LastName { get; set; }
        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        //To Do Virtuals
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();


    }
}
