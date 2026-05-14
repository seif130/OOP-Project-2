using OOP_Project_2.Data;
using OOP_Project_2.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Services
{
    public static class InventoryServices
    {
        public static bool IsSufficient(int branchId, List<OrderItem> items)
            => !GetShortfalls(branchId, items).Any();

        public static Dictionary<int, double> GetShortfalls(int branchId, List<OrderItem> items)
        {
            var required = CalculateRequiredIngredients(items);

            var inventories = Database.BranchInventories
                .Where(i => i.BranchId == branchId)
                .ToDictionary(i => i.IngredientId);

            var shortFalls = new Dictionary<int, double>();

            foreach (var (ingredientId, qty) in required)
            {
                double available = inventories.TryGetValue(ingredientId, out var inv)
                    ? inv.CurrentQuantity
                    : 0;

                if (available < qty)
                    shortFalls[ingredientId] = qty - available;
            }

            return shortFalls;
        }

        public static void Deduct(int branchId, List<OrderItem> items)
        {
            var shortfalls = GetShortfalls(branchId, items);
            if (shortfalls.Any())
                throw new Exception("Insufficient stock");

            var required = CalculateRequiredIngredients(items);

            var inventories = Database.BranchInventories
                .Where(i => i.BranchId == branchId)
                .ToDictionary(i => i.IngredientId);

            foreach (var (ingredientId, qty) in required)
            {
                inventories[ingredientId].CurrentQuantity -= qty;
            }
        }

        private static Dictionary<int, double> CalculateRequiredIngredients(List<OrderItem> items)
        {
            var required = new Dictionary<int, double>();

            foreach (var item in items)
            {
                var recipeItems = Database.RecipeItems
                    .Where(x => x.MenuItemId == item.ItemId);

                foreach (var recipeItem in recipeItems)
                {
                    if (!required.ContainsKey(recipeItem.IngredientId))
                        required[recipeItem.IngredientId] = 0;

                    required[recipeItem.IngredientId] += recipeItem.QuantityRequired * item.Quantity;
                }
            }

            return required;
        }
    }
}
