using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
        
        public DateTime CreateAt{ get; set; }= DateTime.Now;

        public string ShippingAddress { get; set; }


        public User User { get; set; } = null!;
      
      
        public ICollection<OrderItem> OrderItems { get; set; }=new List<OrderItem>();
    }

}
