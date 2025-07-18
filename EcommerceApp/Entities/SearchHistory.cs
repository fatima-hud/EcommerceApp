using Microsoft.AspNetCore.Http.Timeouts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EcommerceApp.Entities
{
    public class SearchHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        public string SearchQuery{ get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.Now;
       
        public User User { get; set; } = null!;
    }
}
