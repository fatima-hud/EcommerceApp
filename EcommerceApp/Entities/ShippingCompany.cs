using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class ShippingCompany
    {
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string PhoneNumber { get; set; }
        public ICollection<Order>Orders { get; set; }=new List<Order>();
    }
}
