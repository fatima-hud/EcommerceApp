﻿namespace EcommerceApp.Entities
{
    public class OtpCode
    {
       
            public int Id { get; set; }
            public string Email { get; set; }
            public string Code { get; set; }
            public DateTime ExpirationTime { get; set; }
        }

    }

