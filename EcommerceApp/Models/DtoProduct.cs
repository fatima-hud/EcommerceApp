namespace EcommerceApp.Models
{
    public class DtoProduct
    {
        public string Name { get; set; }
        public string Description {  get; set; }
        public decimal Price{ get; set; }
        public int CategoryId { get; set; }
        public int Quantity {  get; set; }
        public int? DiscountSettingId {  get; set; }
        public int Rating { get; set; }
        public string ImageUrl {  get; set; }
        public string Gender{ get; set; }
        public string Season {  get; set; }
        public string type {  get; set; }
        public string StyleCloth {  get; set; }
        public string Color {  get; set; }
        public string Size {  get; set; }

    }
}
