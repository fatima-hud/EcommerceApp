using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class User
    {

        public int Id { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string FullName { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        [Required, EmailAddress]
        public string Email { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Password { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string? Phone { get; set; }
        [Column(TypeName = "nvarchar(255)")]
     
        public Boolean IsAdmin { get; set; } = false;
        [Column(TypeName = "nvarchar(50)")]
        public string UserName {  get; set; }
        public string? UserImage {  get; set; }
      

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
      //  public ICollection<Cart> Carts { get; set; } = new List<Cart>();
      public Cart? Cart { get; set; }
        
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime
        {
            get; set;
        }


    }
}
