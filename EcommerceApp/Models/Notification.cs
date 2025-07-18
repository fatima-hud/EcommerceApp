using EcommerceApp.Entities;

namespace EcommerceApp.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public bool IsRead {  get; set; }
        public string Message {  get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserId {  get; set; }
        public User User { get; set; }
    }
}
