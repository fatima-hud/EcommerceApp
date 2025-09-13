namespace EcommerceApp.Entities
{
    public class ColorFamily
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int ColorEntityId { get; set; }
        public ColorEntity Color { get; set; }
    }

}

