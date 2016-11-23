// ASP.NET Core Model binding
using Microsoft.AspNetCore.Mvc.ModelBinding;
// Validation Annotations
using System.ComponentModel.DataAnnotations;

namespace beltexam.ViewModels
{
    // This is a Form Model for the Login page
    public class Login
    {
        [Key]
        [BindNever]
        public int Id { get; private set; }
        [Required]
        [EmailAddressAttribute]
        [Display(Name = "Username")]
        [MinLength(5)]
        [MaxLength(255)]
        public string Email_field { get; set; }
        [Required]
        [Display(Name = "Password")]
        [MinLength(8)]
        [MaxLength(255)]
        [DataType(DataType.Password)]
        public string Password_field { get; set; }
    }
}