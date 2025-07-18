using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public  int OrderId { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }= DateTime.Now;
      

        public Order Order { get; set; } = null!;

    }
}
