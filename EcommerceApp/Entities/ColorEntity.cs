namespace EcommerceApp.Entities
{
    public class ColorEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Hex { get; set; }
        public string Rgb { get; set; }

        public ICollection<ColorFamily> Families { get; set; } = new List<ColorFamily>();
    }

}

