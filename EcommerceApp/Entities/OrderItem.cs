using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity {  get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public String Size {  get; set; }
        [Column(TypeName = "nvarchar(30)")]
        public String Color {  get; set; }
       


        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;

    }
}
