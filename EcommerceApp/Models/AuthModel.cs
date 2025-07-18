namespace EcommerceApp.Models
{
    public class AuthModel
    {
        public String Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public List<String> Roles { get; set; }
        public String Token { get; set; }
        public DateTime Expireson { get; set; }
    }
}
