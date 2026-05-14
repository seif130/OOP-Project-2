using OOP_Project_2.Data;
using OOP_Project_2.Enums;
using OOP_Project_2.Services;

namespace OOP_Project_2.UI;

public static class FeedbackUI
{
    public static void Run()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Feedback");
            ConsoleHelper.PrintOption("1", "Submit Feedback");
            ConsoleHelper.PrintOption("2", "View All Feedback");
            ConsoleHelper.PrintOption("3", "View Feedback by Order");
            ConsoleHelper.PrintOption("0", "Back");

            switch (ConsoleHelper.ReadString("Choice"))
            {
                case "1": Submit();        break;
                case "2": ViewAll();       break;
                case "3": ViewByOrder();   break;
                case "0": return;
                default: ConsoleHelper.Error("Invalid option."); break;
            }
        }
    }

    private static void Submit()
    {
        ConsoleHelper.PrintHeader("Submit Feedback");

        // Show customers
        foreach (var c in Database.Customers) ConsoleHelper.Info($"[{c.CustomerId}] {c.FullName}");
        int custId = ConsoleHelper.ReadInt("Customer ID");

        // Show completed orders for this customer
        var completedOrders = Database.Orders
            .Where(o => o.CustomerId == custId && o.Status == OrderStatus.Completed).ToList();

        if (!completedOrders.Any())
        { ConsoleHelper.Warning("No completed orders found for this customer."); ConsoleHelper.Wait(); return; }

        ConsoleHelper.PrintSubHeader("Completed Orders");
        Console.WriteLine($"\n   {"Order#",-8} {"Date",-20} {"Branch",-22} {"Total"}");
        Console.WriteLine("   " + new string('─', 60));
        foreach (var o in completedOrders)
        {
            var branch = Database.Branches.FirstOrDefault(b => b.BranchId == o.BranchId);
            bool hasFeedback = Database.Feedbacks.Any(f => f.CustomerId == custId && f.OrderId == o.OrderId);
            Console.ForegroundColor = hasFeedback ? ConsoleColor.DarkGray : ConsoleColor.White;
            Console.WriteLine($"   {o.OrderId,-8} {o.DateTime:dd MMM yyyy HH:mm,-20} {branch?.Name ?? "?",-22} {o.TotalAmount:C}{(hasFeedback ? " (reviewed)" : "")}");
            Console.ResetColor();
        }

        int orderId = ConsoleHelper.ReadInt("Order ID");

        ConsoleHelper.PrintSubHeader("Your Rating");
        Console.WriteLine("   [1] ★☆☆☆☆  Poor");
        Console.WriteLine("   [2] ★★☆☆☆  Fair");
        Console.WriteLine("   [3] ★★★☆☆  Good");
        Console.WriteLine("   [4] ★★★★☆  Very Good");
        Console.WriteLine("   [5] ★★★★★  Excellent");
        int rating = ConsoleHelper.ReadInt("Rating", 1, 5);
        string comments = ConsoleHelper.ReadString("Comments (Enter to skip)", true);

        var (success, msg) = FeedbackService.SubmitFeedback(custId, orderId, rating, comments);
        if (success) ConsoleHelper.Success(msg); else ConsoleHelper.Error(msg);
        ConsoleHelper.Wait();
    }

    private static void ViewAll()
    {
        ConsoleHelper.PrintHeader("All Feedback");
        if (!Database.Feedbacks.Any()) { ConsoleHelper.Warning("No feedback yet."); ConsoleHelper.Wait(); return; }

        Console.WriteLine($"\n   {"#",-5} {"Date",-18} {"Customer",-22} {"Order#",-8} {"Rating",-10} {"Comments"}");
        Console.WriteLine("   " + new string('─', 85));
        foreach (var f in Database.Feedbacks.OrderByDescending(x => x.DateTime))
        {
            var cust = Database.Customers.FirstOrDefault(c => c.CustomerId == f.CustomerId);
            Console.ForegroundColor = f.Rating >= 4 ? ConsoleColor.Green : f.Rating >= 3 ? ConsoleColor.Yellow : ConsoleColor.DarkRed;
            Console.WriteLine($"   {f.FeedbackId,-5} {f.DateTime:dd MMM yyyy HH:mm,-18} {cust?.FullName ?? "?",-22} {f.OrderId,-8} {ConsoleHelper.Stars(f.Rating),-10} {f.Comments}");
            Console.ResetColor();
        }
        ConsoleHelper.Wait();
    }

    private static void ViewByOrder()
    {
        ConsoleHelper.PrintHeader("Feedback by Order");
        int orderId = ConsoleHelper.ReadInt("Order ID");
        var fb = Database.Feedbacks.Where(f => f.OrderId == orderId).ToList();
        if (!fb.Any()) { ConsoleHelper.Warning("No feedback found for this order."); ConsoleHelper.Wait(); return; }
        foreach (var f in fb)
        {
            var cust = Database.Customers.FirstOrDefault(c => c.CustomerId == f.CustomerId);
            ConsoleHelper.Row("Customer:",  cust?.FullName);
            ConsoleHelper.Row("Date:",      f.DateTime.ToString("dd MMM yyyy HH:mm"));
            ConsoleHelper.Row("Rating:",    ConsoleHelper.Stars(f.Rating));
            ConsoleHelper.Row("Comments:",  f.Comments);
            Console.WriteLine();
        }
        ConsoleHelper.Wait();
    }
}
