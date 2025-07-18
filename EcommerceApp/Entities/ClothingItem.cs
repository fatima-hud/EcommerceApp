namespace EcommerceApp.Entities
{
    public class ClothingItem
    {
        public int Id { get; set; }
       // public string Type { get; set; }
        public string Style { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int ProductId {  get; set; }
        public Product Product { get; set; }

     //   public string Season { get; set; }
      //  public string Gender { get; set; }
    }
}
