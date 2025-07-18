using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity{ get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string Size { get; set; }
        [Column(TypeName = "nvarchar(30)")]
        public string Color { get; set; }


        
        public Cart Cart { get; set; } = null!;
        
        public Product Product { get; set; } = null!;
    }
}

