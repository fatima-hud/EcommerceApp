using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class EmailCode
    {
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; }

        [Required, MaxLength(10)]
        public string Code { get; set; }

        public DateTime Expiry { get; set; }
    }
}
