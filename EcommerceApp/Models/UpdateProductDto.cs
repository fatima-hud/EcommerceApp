namespace EcommerceApp.Models
{
    public class UpdateProductDto
    {
       public int Id { get; set; }
        public decimal? Price { get; set; }
        public string? ProductName { get; set; }
        public string? Description{ get; set; }

        public int? Quantity { get; set; }
        public int? DiscountSettingId { get; set; }
      
       
    }
}
