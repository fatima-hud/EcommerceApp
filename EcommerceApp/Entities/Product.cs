using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class Product
    {
        public int Id { get; set; }
       
        
        [Column(TypeName = "nvarchar(255)")]
        public string Description { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
       public int? DiscountSettingId { get; set; }
        public int Rating {  get; set; }
        public string Season {  get; set; }
        public string Gender { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Type { get; set; }
        public string Name { get; set; }    
        public string ImageUrl { get; set; }
        public Category Category { get; set; } = null!;
        [Column(TypeName = "nvarchar(100)")]
        public string StyleCloth { get; set; }

        public DiscountSetting? DiscountSetting { get; set; }
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
       // public ICollection<ClothingItem> ClothingItems { get; set;} = new List<ClothingItem>();
    }
}
