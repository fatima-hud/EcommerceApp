using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class DtoEmail
    {
        [Required]
        public string Email { get; set; }
    }
}
