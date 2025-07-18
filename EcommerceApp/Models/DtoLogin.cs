using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class DtoLogin
    {
        [Required]
        public string Email {  get; set; }
        [Required]
        public string Password { get; set; }
    }
}
