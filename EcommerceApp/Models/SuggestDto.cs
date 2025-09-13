namespace EcommerceApp.Models
{
    public class SuggestDto
    {
        public int ClothingitemID { get; set; }
        public string Color{ get; set; }
        public string Size { get; set; }
        public int ProductId {  get; set; }
        public string Image { get; set; }
        public string Style { get; set; }
        public string Type { get; set; }    
        public decimal Price {  get; set; }
        public decimal? Discountsetting { get; set; }
    }
}
