using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Models
{
    public class DtoSignUp
    {
        [Required,Column(TypeName = "nvarchar(100)")]
        public string FullName {  get; set; }
        [Required, Column(TypeName = "nvarchar(50)")]
        public string UserName {  get; set; }
        [Required, Column(TypeName = "nvarchar(100)")]
        public string Email { get; set; }
        [Required, Column(TypeName = "nvarchar(100)")]
        public string Password { get; set; }
        
        [Required, Compare("Password", ErrorMessage = "Passwords do not match."), Column(TypeName = "nvarchar(100)")]
        public string RepeatPassword {  get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string? Phone { get; set; }
    }
}
