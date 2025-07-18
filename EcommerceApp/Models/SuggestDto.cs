namespace EcommerceApp.Models
{
    public class SuggestDto
    {
        public int Id { get; set; }
        public string Color{ get; set; }
        public string Size { get; set; }
        public int ProductId {  get; set; }
        public string Image { get; set; }
        public string Style { get; set; }
        public decimal Price {  get; set; }
    }
}
