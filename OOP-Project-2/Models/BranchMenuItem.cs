using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class BranchMenuItem
    {
        public int BranchId { get; set; }
        public int ItemId { get; set; }
        public bool IsAvailable { get; set; }
        public decimal? PriceOverride { get; set; }
    }
}
