using OOP_Project_2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class Delivery
    {
        public int DeliveryId { get; set; }
        public int OrderId { get; set; }
     
        public DateTime DateTime { get; set; }
        public DeliveyStatus Status { get; set; } = DeliveyStatus.AwaitingPickup;
        public string? DeliveryAddress { get; set; }

        public string? FailureReason { get; set; }

        public int? DeliveryStaffId { get; set; }
    }
}
