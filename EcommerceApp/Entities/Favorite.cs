using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class Favorite
    {
        public int Id { get; set; }
        public int UserId {  get; set; }
        public int ProductId {  get; set; }
       


        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
