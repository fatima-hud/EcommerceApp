namespace EcommerceApp.SeedData
{
    public class ColorJsonModel
    {
        public string Name { get; set; }
        public string Hex { get; set; }
        public string Rgb { get; set; }
        public List<string> Families { get; set; } = new();
    }

}
