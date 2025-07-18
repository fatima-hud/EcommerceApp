using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class DiscountSetting
    {
        public int Id { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountPercentage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
