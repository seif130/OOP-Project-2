using OOP_Project_2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public OrderType OrderType { get; set; }
        public DateTime DateTime { get; set; }  
        public decimal TotalAmount { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public int BranchId { get; set; }
        public int HandledByEmployeeId { get; set; }

        public int CustomerId { get; set; }

        public string? DeliveryAddress { get; set; }

        public List<OrderItem> Items { get; set; }
    }
}
