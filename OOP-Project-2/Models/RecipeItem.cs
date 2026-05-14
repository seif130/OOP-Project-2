using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Models
{
    public class RecipeItem
    {
        public int MenuItemId { get; set; }

        public int IngredientId { get; set; }

        public double QuantityRequired { get; set; }
    }
}
