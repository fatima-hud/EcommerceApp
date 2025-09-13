using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class SendOtpRequest
    {
        [Required, EmailAddress]
        public string Email {  get; set; }
    }
}
