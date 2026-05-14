using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string? Comments { get; set; }
        public int Rating { get; set; }
        public DateTime DateTime { get; set; }
    }
}
