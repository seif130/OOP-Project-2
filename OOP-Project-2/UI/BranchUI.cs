using OOP_Project_2.Data;
using OOP_Project_2.Models;

namespace OOP_Project_2.UI;

public static class BranchUI
{
    public static void Run()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Branch Management");
            ConsoleHelper.PrintOption("1", "List All Branches");
            ConsoleHelper.PrintOption("2", "View Branch Details");
            ConsoleHelper.PrintOption("3", "Add New Branch");
            ConsoleHelper.PrintOption("4", "View Branch Menu");
            ConsoleHelper.PrintOption("5", "Manage Branch Menu Availability");
            ConsoleHelper.PrintOption("0", "Back");

            var choice = ConsoleHelper.ReadString("Choice");
            switch (choice)
            {
                case "1": ListBranches();          break;
                case "2": ViewBranch();            break;
                case "3": AddBranch();             break;
                case "4": ViewBranchMenu();        break;
                case "5": ManageBranchMenu();      break;
                case "0": return;
                default:  ConsoleHelper.Error("Invalid option."); break;
            }
        }
    }

    private static void ListBranches()
    {
        ConsoleHelper.PrintHeader("All Branches");
        if (!Database.Branches.Any()) { ConsoleHelper.Warning("No branches found."); ConsoleHelper.Wait(); return; }
        Console.WriteLine($"\n   {"ID",-5} {"Name",-25} {"Contact",-15} {"Hours",-15} {"Manager"}");
        Console.WriteLine("   " + new string('─', 75));
        foreach (var b in Database.Branches)
        {
            var mgr = Database.Employees.FirstOrDefault(e => e.EmployeeId == b.ManagerId);
            Console.WriteLine($"   {b.BranchId,-5} {b.Name,-25} {b.ContactNumber,-15} {b.OpeningHours,-15} {mgr?.FullName ?? "N/A"}");
        }
        ConsoleHelper.Wait();
    }

    private static void ViewBranch()
    {
        ConsoleHelper.PrintHeader("View Branch Details");
        int id = ConsoleHelper.ReadInt("Branch ID");
        var b = Database.Branches.FirstOrDefault(x => x.BranchId == id);
        if (b is null) { ConsoleHelper.Error("Branch not found."); ConsoleHelper.Wait(); return; }

        var mgr = Database.Employees.FirstOrDefault(e => e.EmployeeId == b.ManagerId);
        ConsoleHelper.PrintSubHeader("Branch Information");
        ConsoleHelper.Row("Branch ID:",     b.BranchId);
        ConsoleHelper.Row("Name:",          b.Name);
        ConsoleHelper.Row("Address:",       b.Address);
        ConsoleHelper.Row("Contact:",       b.ContactNumber);
        ConsoleHelper.Row("Opening Hours:", b.OpeningHours);
        ConsoleHelper.Row("Manager:",       mgr?.FullName ?? "Unassigned");

        ConsoleHelper.PrintSubHeader("Staff at this Branch");
        var staff = Database.Employees.Where(e => e.AssignmedBranchIds.Contains(id)).ToList();
        if (staff.Any())
            foreach (var e in staff)
                ConsoleHelper.Info($"[{e.EmployeeId}] {e.FullName,-22} {e.GetRole()}");
        else
            ConsoleHelper.Info("No employees assigned.");

        ConsoleHelper.Wait();
    }

    private static void AddBranch()
    {
        ConsoleHelper.PrintHeader("Add New Branch");
        int newId = Database.Branches.Any() ? Database.Branches.Max(b => b.BranchId) + 1 : 1;
        var name    = ConsoleHelper.ReadString("Branch Name");
        var address = ConsoleHelper.ReadString("Address");
        var contact = ConsoleHelper.ReadString("Contact Number");
        var hours   = ConsoleHelper.ReadString("Opening Hours (e.g. 08:00-22:00)");

        // Pick manager from existing BranchManagers
        var managers = Database.Employees.OfType<BranchManger>().ToList();
        int managerId = 0;
        if (managers.Any())
        {
            ConsoleHelper.PrintSubHeader("Available Branch Managers");
            foreach (var m in managers) ConsoleHelper.Info($"[{m.EmployeeId}] {m.FullName}");
            managerId = ConsoleHelper.ReadInt("Assign Manager ID (0 to skip)", 0);
        }

        Database.Branches.Add(new Branch
        {
            BranchId      = newId,
            Name          = name,
            Address       = address,
            ContactNumber = contact,
            OpeningHours  = hours,
            ManagerId     = managerId
        });
        ConsoleHelper.Success($"Branch '{name}' added with ID {newId}.");
        ConsoleHelper.Wait();
    }

    private static void ViewBranchMenu()
    {
        ConsoleHelper.PrintHeader("Branch Menu");
        int branchId = ConsoleHelper.ReadInt("Branch ID");
        var branch = Database.Branches.FirstOrDefault(b => b.BranchId == branchId);
        if (branch is null) { ConsoleHelper.Error("Branch not found."); ConsoleHelper.Wait(); return; }

        ConsoleHelper.PrintSubHeader($"Menu for {branch.Name}");
        Console.WriteLine($"\n   {"ID",-5} {"Name",-22} {"Category",-15} {"Base Price",-12} {"Branch Price",-13} {"Available"}");
        Console.WriteLine("   " + new string('_' , 78));
        foreach (var mi in Database.MenuItems)
        {
            var bmi = Database.BranchMenuItems.FirstOrDefault(b => b.BranchId == branchId && b.ItemId == mi.ItemId);
            string avail = bmi?.IsAvailable == true ? "Yes" : "No";
            string price = bmi?.PriceOverride.HasValue == true ? $"{bmi.PriceOverride:C}" : "—";
            Console.ForegroundColor = bmi?.IsAvailable == true ? ConsoleColor.White : ConsoleColor.DarkGray;
            Console.WriteLine($"   {mi.ItemId,-5} {mi.Name,-22} {mi.Category,-15} {mi.BasePrice,-12:C} {price,-13} {avail}");
            Console.ResetColor();
        }
        ConsoleHelper.Wait();
    }

    private static void ManageBranchMenu()
    {
        ConsoleHelper.PrintHeader("Manage Branch Menu Availability");
        int branchId = ConsoleHelper.ReadInt("Branch ID");
        var branch = Database.Branches.FirstOrDefault(b => b.BranchId == branchId);
        if (branch is null) { ConsoleHelper.Error("Branch not found."); ConsoleHelper.Wait(); return; }

        int itemId = ConsoleHelper.ReadInt("Menu Item ID");
        var menuItem = Database.MenuItems.FirstOrDefault(m => m.ItemId == itemId);
        if (menuItem is null) { ConsoleHelper.Error("Menu item not found."); ConsoleHelper.Wait(); return; }

        var bmi = Database.BranchMenuItems.FirstOrDefault(b => b.BranchId == branchId && b.ItemId == itemId);
        if (bmi is null)
        {
            bmi = new BranchMenuItem { BranchId = branchId, ItemId = itemId };
            Database.BranchMenuItems.Add(bmi);
        }

        ConsoleHelper.Row("Item:", menuItem.Name);
        ConsoleHelper.Row("Currently Available:", bmi.IsAvailable);
        ConsoleHelper.Row("Branch Price Override:", bmi.PriceOverride?.ToString("C") ?? "None (uses base)");

        bmi.IsAvailable = ConsoleHelper.Confirm("Set as available?");

        if (ConsoleHelper.Confirm("Set a price override?"))
            bmi.PriceOverride = ConsoleHelper.ReadDecimal("Override Price");
        else
            bmi.PriceOverride = null;

        ConsoleHelper.Success("Branch menu item updated.");
        ConsoleHelper.Wait();
    }
}
