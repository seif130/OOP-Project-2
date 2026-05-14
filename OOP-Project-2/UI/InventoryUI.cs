using OOP_Project_2.Data;

namespace OOP_Project_2.UI;

public static class InventoryUI
{
    public static void Run()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Inventory Management");
            ConsoleHelper.PrintOption("1", "View Branch Inventory");
            ConsoleHelper.PrintOption("2", "Restock Ingredient");
            ConsoleHelper.PrintOption("3", "View All Ingredients");
            ConsoleHelper.PrintOption("0", "Back");

            switch (ConsoleHelper.ReadString("Choice"))
            {
                case "1": ViewInventory();  break;
                case "2": Restock();        break;
                case "3": ListIngredients(); break;
                case "0": return;
                default: ConsoleHelper.Error("Invalid option."); break;
            }
        }
    }

    private static void ViewInventory()
    {
        ConsoleHelper.PrintHeader("Branch Inventory");
        foreach (var b in Database.Branches) ConsoleHelper.Info($"[{b.BranchId}] {b.Name}");
        int branchId = ConsoleHelper.ReadInt("Branch ID");
        var branch = Database.Branches.FirstOrDefault(b => b.BranchId == branchId);
        if (branch is null) { ConsoleHelper.Error("Branch not found."); ConsoleHelper.Wait(); return; }

        ConsoleHelper.PrintSubHeader($"Inventory — {branch.Name}");
        Console.WriteLine($"\n   {"ID",-5} {"Ingredient",-22} {"Unit",-10} {"Quantity",-12} {"Status"}");
        Console.WriteLine("   " + new string('─', 60));

        foreach (var ing in Database.Ingredients)
        {
            var inv = Database.BranchInventories.FirstOrDefault(i => i.BranchId == branchId && i.IngredientId == ing.IngredientId);
            double qty = inv?.CurrentQuantity ?? 0;
            string status;
            ConsoleColor color;
            if (qty <= 0)       { status = "OUT OF STOCK"; color = ConsoleColor.Red; }
            else if (qty < 2)   { status = "LOW";          color = ConsoleColor.Yellow; }
            else                { status = "OK";           color = ConsoleColor.Green; }

            Console.Write($"   {ing.IngredientId,-5} {ing.Name,-22} {ing.Unit,-10} {qty,-12:F2} ");
            Console.ForegroundColor = color;
            Console.WriteLine(status);
            Console.ResetColor();
        }
        ConsoleHelper.Wait();
    }

    private static void Restock()
    {
        ConsoleHelper.PrintHeader("Restock Ingredient");
        foreach (var b in Database.Branches) ConsoleHelper.Info($"[{b.BranchId}] {b.Name}");
        int branchId = ConsoleHelper.ReadInt("Branch ID");
        if (!Database.Branches.Any(b => b.BranchId == branchId))
        { ConsoleHelper.Error("Branch not found."); ConsoleHelper.Wait(); return; }

        ListIngredients();
        int ingId = ConsoleHelper.ReadInt("Ingredient ID");
        var ing = Database.Ingredients.FirstOrDefault(i => i.IngredientId == ingId);
        if (ing is null) { ConsoleHelper.Error("Ingredient not found."); ConsoleHelper.Wait(); return; }

        Console.Write($"\n  Amount to add ({ing.Unit}): ");
        if (!double.TryParse(Console.ReadLine(), out double amount) || amount <= 0)
        { ConsoleHelper.Error("Invalid amount."); ConsoleHelper.Wait(); return; }

        var inv = Database.BranchInventories.FirstOrDefault(i => i.BranchId == branchId && i.IngredientId == ingId);
        if (inv is null)
        {
            inv = new Models.BranchInventory { BranchId = branchId, IngredientId = ingId };
            Database.BranchInventories.Add(inv);
        }
        inv.CurrentQuantity += amount;
        ConsoleHelper.Success($"{ing.Name} restocked. New quantity: {inv.CurrentQuantity:F2} {ing.Unit}");
        ConsoleHelper.Wait();
    }

    private static void ListIngredients()
    {
        Console.WriteLine($"\n   {"ID",-5} {"Name",-22} {"Unit"}");
        Console.WriteLine("   " + new string('─', 35));
        foreach (var i in Database.Ingredients)
            Console.WriteLine($"   {i.IngredientId,-5} {i.Name,-22} {i.Unit}");
    }
}
