using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class MenuItem
    {
        public int ItemId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal BasePrice { get; set; }

        public string Category { get; set; }

        public List<AddOn> AddOns { get; set; } = new List<AddOn>();
     
    }
}
