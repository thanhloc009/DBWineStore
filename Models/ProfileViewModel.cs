using System.ComponentModel.DataAnnotations;
using System.Web;

namespace DBWineStore.Models
{
    public class ProfileViewModel
    {
        public int UserID { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter full name")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [StringLength(250)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Role")]
        public string RoleName { get; set; }

        public string AvatarUrl { get; set; }

        public HttpPostedFileBase AvatarFile { get; set; }

        public bool RemoveAvatar { get; set; }
    }
}