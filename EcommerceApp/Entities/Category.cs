using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class Category
    {
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public String Name { get; set; }
       
        public ICollection<Product>Products { get; set; }=new List<Product>();
    }
}
