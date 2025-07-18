using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string text { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }


        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;
        
    }
}
