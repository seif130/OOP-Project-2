using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string FullName { get; set; } 

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public int LoyaltyPoints { get; set; }
    }
}
