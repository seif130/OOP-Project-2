
using OOP_Project_2.Data;
using OOP_Project_2.Models;

namespace OOP_Project_2.UI;

public static class CustomerUI
{
    public static void Run()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Customer Management");
            ConsoleHelper.PrintOption("1", "List All Customers");
            ConsoleHelper.PrintOption("2", "View Customer Details");
            ConsoleHelper.PrintOption("3", "Register New Customer");
            ConsoleHelper.PrintOption("4", "Customer Order History");
            ConsoleHelper.PrintOption("0", "Back");

            switch (ConsoleHelper.ReadString("Choice"))
            {
                case "1": ListCustomers();       break;
                case "2": ViewCustomer();        break;
                case "3": RegisterCustomer();    break;
                case "4": OrderHistory();        break;
                case "0": return;
                default: ConsoleHelper.Error("Invalid option."); break;
            }
        }
    }

    private static void ListCustomers()
    {
        ConsoleHelper.PrintHeader("All Customers");
        Console.WriteLine($"\n   {"ID",-6} {"Name",-24} {"Phone",-15} {"Email",-28} {"Points"}");
        Console.WriteLine("   " + new string('─', 75));
        foreach (var c in Database.Customers)
            Console.WriteLine($"   {c.CustomerId,-6} {c.FullName,-24} {c.PhoneNumber,-15} {c.Email,-28} {c.LoyaltyPoints}");
        ConsoleHelper.Wait();
    }

    private static void ViewCustomer()
    {
        ConsoleHelper.PrintHeader("Customer Details");
        int id = ConsoleHelper.ReadInt("Customer ID");
        var c = Database.Customers.FirstOrDefault(x => x.CustomerId == id);
        if (c is null) { ConsoleHelper.Error("Customer not found."); ConsoleHelper.Wait(); return; }

        ConsoleHelper.Row("Customer ID:",   c.CustomerId);
        ConsoleHelper.Row("Name:",          c.FullName);
        ConsoleHelper.Row("Phone:",         c.PhoneNumber);
        ConsoleHelper.Row("Email:",         c.Email);
        ConsoleHelper.Row("Loyalty Points:", c.LoyaltyPoints);

        var orders = Database.Orders.Where(o => o.CustomerId == id).ToList();
        ConsoleHelper.Row("Total Orders:",  orders.Count);
        ConsoleHelper.Row("Total Spent:",   orders.Where(o => o.Status == OOP_Project_2.Enums.OrderStatus.Completed).Sum(o => o.TotalAmount).ToString("C"));
        ConsoleHelper.Wait();
    }

    private static void RegisterCustomer()
    {
        ConsoleHelper.PrintHeader("Register New Customer");
        string name  = ConsoleHelper.ReadString("Full Name");
        string phone = ConsoleHelper.ReadString("Phone Number");
        string email = ConsoleHelper.ReadString("Email");
        int newId = Database.NextCustomerId++;
        Database.Customers.Add(new Customer { CustomerId = newId, FullName = name, PhoneNumber = phone, Email = email });
        ConsoleHelper.Success($"Customer '{name}' registered with ID {newId}.");
        ConsoleHelper.Wait();
    }

    private static void OrderHistory()
    {
        ConsoleHelper.PrintHeader("Customer Order History");
        int id = ConsoleHelper.ReadInt("Customer ID");
        var c = Database.Customers.FirstOrDefault(x => x.CustomerId == id);
        if (c is null) { ConsoleHelper.Error("Customer not found."); ConsoleHelper.Wait(); return; }

        var orders = Database.Orders.Where(o => o.CustomerId == id).OrderByDescending(o => o.DateTime).ToList();
        ConsoleHelper.PrintSubHeader($"Orders for {c.FullName}");
        if (!orders.Any()) { ConsoleHelper.Warning("No orders found."); ConsoleHelper.Wait(); return; }

        Console.WriteLine($"\n   {"Order#",-8} {"Date",-20} {"Type",-12} {"Total",-10} {"Status"}");
        Console.WriteLine("   " + new string('─', 65));
        foreach (var o in orders)
        {
            var color = o.Status switch {
                OOP_Project_2.Enums.OrderStatus.Completed => ConsoleColor.Green,
                OOP_Project_2.Enums.OrderStatus.Cancelled  => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };
            Console.ForegroundColor = color;
            Console.WriteLine($"   {o.OrderId,-8} {o.DateTime:dd MMM yyyy HH:mm,-20} {o.OrderType,-12} {o.TotalAmount,-10:C} {o.Status}");
            Console.ResetColor();
        }
        ConsoleHelper.Wait();
    }
}
