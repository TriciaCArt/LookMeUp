using System.ComponentModel.DataAnnotations;

namespace LookMeUp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? AppUserID { get; set; }

        [Required]
        [Display(Name="Category Name")]
        public string? Name { get; set; }

        //ToDo: Add Virtuals
        public virtual AppUser? AppUser { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
    }
}
