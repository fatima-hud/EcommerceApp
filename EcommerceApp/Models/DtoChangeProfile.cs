namespace EcommerceApp.Models
{
    public class DtoChangeProfile
    {
        public string? FullName { get; set; }

        // اسم المستخدم الجديد (optional)
        public string? UserName { get; set; }

        // الايميل الجديد (optional)
        public string? Email { get; set; }

        // رقم الهاتف الجديد (optional)
        public string? PhoneNumber { get; set; }
    }
}
