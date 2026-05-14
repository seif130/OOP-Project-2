using OOP_Project_2.Data;
using OOP_Project_2.Enums;
using OOP_Project_2.Models;
using OOP_Project_2.Services;

namespace OOP_Project_2.UI;

public static class OrderUI
{
    public static void Run()
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Order Management");
            ConsoleHelper.PrintOption("1", "Place New Order");
            ConsoleHelper.PrintOption("2", "Start Preparing   [Chef]");
            ConsoleHelper.PrintOption("3", "Mark as Served    [Chef]");
            ConsoleHelper.PrintOption("4", "Process Payment   [Cashier]");
            ConsoleHelper.PrintOption("5", "Cancel Order");
            ConsoleHelper.PrintOption("6", "View Order Details");
            ConsoleHelper.PrintOption("7", "List Orders");
            ConsoleHelper.PrintOption("0", "Back");

            switch (ConsoleHelper.ReadString("Choice"))
            {
                case "1": PlaceOrder();       break;
                case "2": StartPreparing();   break;
                case "3": ServeOrder();       break;
                case "4": ProcessPayment();   break;
                case "5": CancelOrder();      break;
                case "6": ViewOrder();        break;
                case "7": ListOrders();       break;
                case "0": return;
                default: ConsoleHelper.Error("Invalid option."); break;
            }
        }
    }

    // ── Place Order ─────────────────────────────────────────────────────────────
    private static void PlaceOrder()
    {
        ConsoleHelper.PrintHeader("Place New Order");

        // Employee
        ConsoleHelper.PrintSubHeader("Step 1: Employee (Waiter or Cashier)");
        foreach (var e in Database.Employees.Where(e => e is Waiter || e is Cashier))
            ConsoleHelper.Info($"[{e.EmployeeId}] {e.FullName,-22} {e.GetRole(),-10} Branch: {e.PrimaryBranchId}");
        int empId = ConsoleHelper.ReadInt("Employee ID");

        // Customer
        ConsoleHelper.PrintSubHeader("Step 2: Customer");
        foreach (var c in Database.Customers)
            ConsoleHelper.Info($"[{c.CustomerId}] {c.FullName,-22} Points: {c.LoyaltyPoints}");
        int custId = ConsoleHelper.ReadInt("Customer ID");

        // Branch
        ConsoleHelper.PrintSubHeader("Step 3: Branch");
        foreach (var b in Database.Branches)
            ConsoleHelper.Info($"[{b.BranchId}] {b.Name}");
        int branchId = ConsoleHelper.ReadInt("Branch ID");

        // Order Type
        ConsoleHelper.PrintSubHeader("Step 4: Order Type");
        ConsoleHelper.PrintOption("1", "Dine-In");
        ConsoleHelper.PrintOption("2", "Takeaway");
        ConsoleHelper.PrintOption("3", "Delivery");
        int typeChoice = ConsoleHelper.ReadInt("Type", 1, 3);
        var orderType = typeChoice switch { 1 => OrderType.DineIn, 2 => OrderType.TakeAway, _ => OrderType.Delivery };

        string? deliveryAddr = null;
        if (orderType == OrderType.Delivery)
            deliveryAddr = ConsoleHelper.ReadString("Delivery Address");

        // Items
        ConsoleHelper.PrintSubHeader("Step 5: Add Items");
        ShowBranchMenu(branchId);
        var items = new List<(int ItemId, int Qty, string? Notes, List<int> AddOnIds)>();

        while (true)
        {
            int itemId = ConsoleHelper.ReadInt("Item ID (0 to finish)", 0);
            if (itemId == 0) { if (items.Count == 0) { ConsoleHelper.Warning("Add at least one item."); continue; } break; }

            var mi = Database.MenuItems.FirstOrDefault(m => m.ItemId == itemId);
            if (mi is null) { ConsoleHelper.Error("Item not found."); continue; }
            var bmi = Database.BranchMenuItems.FirstOrDefault(b => b.BranchId == branchId && b.ItemId == itemId);
            if (bmi is null || !bmi.IsAvailable) { ConsoleHelper.Error("Item not available at this branch."); continue; }

            int qty = ConsoleHelper.ReadInt("Quantity", 1, 99);
            string notes = ConsoleHelper.ReadString("Special Notes (Enter to skip)", true);

            var selectedAddOns = new List<int>();
            if (mi.AddOns.Any())
            {
                ConsoleHelper.PrintSubHeader("Available Add-Ons");
                foreach (var a in mi.AddOns) ConsoleHelper.Info($"[{a.AddOnId}] {a.Name,-20} +{a.ExtraPrice:C}");
                Console.Write("\n  Add-On IDs (comma-separated, Enter to skip): ");
                var addOnInput = Console.ReadLine() ?? "";
                foreach (var part in addOnInput.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    if (int.TryParse(part.Trim(), out int aId) && mi.AddOns.Any(a => a.AddOnId == aId))
                        selectedAddOns.Add(aId);
            }
            items.Add((itemId, qty, string.IsNullOrWhiteSpace(notes) ? null : notes, selectedAddOns));
            ConsoleHelper.Success($"Added: {mi.Name} x{qty}");
        }

        // Order summary
        ConsoleHelper.PrintSubHeader("Order Summary");
        decimal preview = 0;
        foreach (var (iId, qty, notes, addOns) in items)
        {
            var mi = Database.MenuItems.First(m => m.ItemId == iId);
            var bmi = Database.BranchMenuItems.First(b => b.BranchId == branchId && b.ItemId == iId);
            decimal price = bmi.PriceOverride ?? mi.BasePrice;
            foreach (var aId in addOns) price += mi.AddOns.FirstOrDefault(a => a.AddOnId == aId)?.ExtraPrice ?? 0;
            ConsoleHelper.Info($"{mi.Name,-22} x{qty} @ {price:C} = {price * qty:C}");
            if (notes != null) ConsoleHelper.Info($"  Note: {notes}");
            preview += price * qty;
        }
        ConsoleHelper.Info($"\n  Estimated Total: {preview:C}");

        if (!ConsoleHelper.Confirm("Confirm order?")) { ConsoleHelper.Warning("Order canceled."); ConsoleHelper.Wait(); return; }

        var (success, msg) = OrderServices.PlaceOrder(empId, custId, branchId, orderType, deliveryAddr, items);
        if (success) ConsoleHelper.Success($"{msg}");
        else         ConsoleHelper.Error(msg);
        ConsoleHelper.Wait();
    }

    // ── Start Preparing ─────────────────────────────────────────────────────────
    private static void StartPreparing()
    {
        ConsoleHelper.PrintHeader("Start Preparing Order");
        ShowPendingOrders();
        int orderId = ConsoleHelper.ReadInt("Order ID");
        ConsoleHelper.PrintSubHeader("Chef Authentication");
        foreach (var e in Database.Employees.OfType<Chef>())
            ConsoleHelper.Info($"[{e.EmployeeId}] {e.FullName,-22} Branch: {e.PrimaryBranchId}");
        int chefId = ConsoleHelper.ReadInt("Chef ID");

        var (success, msg) = OrderServices.StartPreparing(orderId, chefId);

        if (!success && msg == "INSUFFICIENT_STOCK")
        {
            // Show shortfalls
            var order = Database.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order is not null)
            {
                ConsoleHelper.Warning("Insufficient stock detected!");
                var shortfalls = InventoryServices.GetShortfalls(order.BranchId, order.Items);
                foreach (var (ingId, deficit) in shortfalls)
                {
                    var ing = Database.Ingredients.FirstOrDefault(i => i.IngredientId == ingId);
                    ConsoleHelper.Info($"  {ing?.Name ?? "?",-20} short by {deficit:F2} {ing?.Unit}");
                }
            }

            if (ConsoleHelper.Confirm("Request Branch Manager override?"))
            {
                var managers = Database.Employees.OfType<BranchManger>().ToList();
                foreach (var m in managers) ConsoleHelper.Info($"[{m.EmployeeId}] {m.FullName}");
                int mgr = ConsoleHelper.ReadInt("Manager ID");
                var (s2, m2) = OrderServices.StartPreparing(orderId, chefId);
                if (s2) ConsoleHelper.Success(m2); else ConsoleHelper.Error(m2);
            }
            else ConsoleHelper.Warning("Preparation blocked: insufficient stock.");
        }
        else if (success) ConsoleHelper.Success(msg);
        else ConsoleHelper.Error(msg);

        ConsoleHelper.Wait();
    }

    // ── Serve Order ─────────────────────────────────────────────────────────────
    private static void ServeOrder()
    {
        ConsoleHelper.PrintHeader("Mark Order as Served");
        ShowOrdersByStatus(OrderStatus.InProgress);
        int orderId = ConsoleHelper.ReadInt("Order ID");
        ConsoleHelper.PrintSubHeader("Chef Authentication");
        foreach (var e in Database.Employees.OfType<Chef>())
            ConsoleHelper.Info($"[{e.EmployeeId}] {e.FullName,-22} Branch: {e.PrimaryBranchId}");
        int chefId = ConsoleHelper.ReadInt("Chef ID");

        var (success, msg) = OrderServices.ServeOrder(orderId, chefId);
        if (success) ConsoleHelper.Success(msg); else ConsoleHelper.Error(msg);
        ConsoleHelper.Wait();
    }

    // ── Process Payment ─────────────────────────────────────────────────────────
    private static void ProcessPayment()
    {
        ConsoleHelper.PrintHeader("Process Payment");
        ShowOrdersByStatus(OrderStatus.Served);
        int orderId = ConsoleHelper.ReadInt("Order ID");

        var order = Database.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order is null) { ConsoleHelper.Error("Order not found."); ConsoleHelper.Wait(); return; }
        if (order.OrderType == OrderType.Delivery)
        {
            ConsoleHelper.Warning("Delivery orders are completed via the delivery flow, not cashier payment.");
            ConsoleHelper.Wait(); return;
        }

        ConsoleHelper.PrintSubHeader("Cashier Authentication");
        foreach (var e in Database.Employees.OfType<Cashier>())
            ConsoleHelper.Info($"[{e.EmployeeId}] {e.FullName,-22} Branch: {e.PrimaryBranchId}");
        int cashierId = ConsoleHelper.ReadInt("Cashier ID");

        ConsoleHelper.PrintSubHeader("Payment Method");
        ConsoleHelper.PrintOption("1", "Cash");
        ConsoleHelper.PrintOption("2", "Card");
        ConsoleHelper.PrintOption("3", "Wallet");
        int pm = ConsoleHelper.ReadInt("Method", 1, 3);
        var method = pm switch { 1 => PaymentMethod.Cash, 2 => PaymentMethod.Card, _ => PaymentMethod.Wallet };

        ConsoleHelper.Info($"Total: {order.TotalAmount:C}");
        var (success, msg) = OrderServices.ProcessPayment(orderId, cashierId, method);
        if (success) ConsoleHelper.Success(msg); else ConsoleHelper.Error(msg);
        ConsoleHelper.Wait();
    }

    // ── Cancel Order ────────────────────────────────────────────────────────────
    private static void CancelOrder()
    {
        ConsoleHelper.PrintHeader("Cancel Order");
        ListOrders();
        int orderId = ConsoleHelper.ReadInt("Order ID");
        ConsoleHelper.PrintSubHeader("Employee Authentication");
        foreach (var e in Database.Employees.Where(e => e is Waiter || e is Cashier || e is BranchManger))
            ConsoleHelper.Info($"[{e.EmployeeId}] {e.FullName,-22} {e.GetRole()}");
        int empId = ConsoleHelper.ReadInt("Employee ID");

        var (success, msg) = OrderServices.CancelOrder(orderId, empId);
        if (success) ConsoleHelper.Success(msg); else ConsoleHelper.Error(msg);
        ConsoleHelper.Wait();
    }

    // ── View Order ──────────────────────────────────────────────────────────────
    public static void ViewOrder()
    {
        ConsoleHelper.PrintHeader("Order Details");
        int orderId = ConsoleHelper.ReadInt("Order ID");
        var o = Database.Orders.FirstOrDefault(x => x.OrderId == orderId);
        if (o is null) { ConsoleHelper.Error("Order not found."); ConsoleHelper.Wait(); return; }

        PrintOrderDetail(o);
        ConsoleHelper.Wait();
    }

    public static void PrintOrderDetail(Order o)
    {
        var customer  = Database.Customers.FirstOrDefault(c => c.CustomerId == o.CustomerId);
        var employee  = Database.Employees.FirstOrDefault(e => e.EmployeeId == o.HandledByEmployeeId);
        var branch    = Database.Branches.FirstOrDefault(b => b.BranchId == o.BranchId);

        ConsoleHelper.PrintSubHeader("Order Information");
        ConsoleHelper.Row("Order ID:",      $"#{o.OrderId}");
        ConsoleHelper.Row("Date/Time:",     o.DateTime.ToString("dd MMM yyyy  HH:mm"));
        ConsoleHelper.Row("Type:",          o.OrderType);
        ConsoleHelper.Row("Status:",        o.Status);
        ConsoleHelper.Row("Branch:",        branch?.Name);
        ConsoleHelper.Row("Customer:",      customer?.FullName);
        ConsoleHelper.Row("Handled By:",    employee?.FullName);
        if (o.OrderType == OrderType.Delivery)
            ConsoleHelper.Row("Delivery Address:", o.DeliveryAddress);
        ConsoleHelper.Row("Payment:",       o.PaymentMethod?.ToString() ?? "Pending");
        ConsoleHelper.Row("Total:",         o.TotalAmount.ToString("C"));

        ConsoleHelper.PrintSubHeader("Items");
        Console.WriteLine($"   {"#",-4} {"Item",-22} {"Qty",-6} {"Unit Price",-12} {"Notes"}");
        Console.WriteLine("   " + new string('─', 65));
        foreach (var item in o.Items)
        {
            var mi = Database.MenuItems.FirstOrDefault(m => m.ItemId == item.ItemId);
            string addOns = item.SelectedAddOnIds.Any()
                ? " [+" + string.Join(", ", item.SelectedAddOnIds.Select(id => mi?.AddOns.FirstOrDefault(a => a.AddOnId == id)?.Name ?? "?")) + "]"
                : "";
            Console.WriteLine($"   {item.OrderItemId,-4} {(mi?.Name ?? "?")+addOns,-22} {item.Quantity,-6} {item.UnitPrice,-12:C} {item.SpecialNotes}");
        }
    }

    // ── List Orders ─────────────────────────────────────────────────────────────
    public static void ListOrders()
    {
        ConsoleHelper.PrintHeader("All Orders");
        if (!Database.Orders.Any()) { ConsoleHelper.Warning("No orders found."); ConsoleHelper.Wait(); return; }
        PrintOrderTable(Database.Orders.OrderByDescending(o => o.DateTime).ToList());
        ConsoleHelper.Wait();
    }

    private static void ShowPendingOrders()
        => PrintOrderTable(Database.Orders.Where(o => o.Status == OrderStatus.Pending).ToList());

    private static void ShowOrdersByStatus(OrderStatus status)
        => PrintOrderTable(Database.Orders.Where(o => o.Status == status).ToList());

    private static void PrintOrderTable(List<Order> orders)
    {
        if (!orders.Any()) { ConsoleHelper.Warning("No matching orders."); return; }
        Console.WriteLine($"\n   {"ID",-6} {"Date",-18} {"Type",-11} {"Branch",-20} {"Customer",-20} {"Total",-10} {"Status"}");
        Console.WriteLine("   " + new string('─', 95));
        foreach (var o in orders)
        {
            var cust  = Database.Customers.FirstOrDefault(c => c.CustomerId == o.CustomerId);
            var branch = Database.Branches.FirstOrDefault(b => b.BranchId == o.BranchId);
            var color = o.Status switch {
                OrderStatus.Completed => ConsoleColor.Green,
                OrderStatus.Cancelled  => ConsoleColor.DarkRed,
                OrderStatus.Served    => ConsoleColor.Yellow,
                OrderStatus.InProgress => ConsoleColor.Cyan,
                _ => ConsoleColor.White
            };
            Console.ForegroundColor = color;
            Console.WriteLine($"   {o.OrderId,-6} {o.DateTime:dd MMM HH:mm,-18} {o.OrderType,-11} {branch?.Name ?? "?",-20} {cust?.FullName ?? "?",-20} {o.TotalAmount,-10:C} {o.Status}");
            Console.ResetColor();
        }
    }

    private static void ShowBranchMenu(int branchId)
    {
        ConsoleHelper.PrintSubHeader("Available Menu Items");
        Console.WriteLine($"\n   {"ID",-5} {"Name",-22} {"Category",-14} {"Price",-10} {"Add-Ons"}");
        Console.WriteLine("   " + new string('─', 65));
        foreach (var mi in Database.MenuItems)
        {
            var bmi = Database.BranchMenuItems.FirstOrDefault(b => b.BranchId == branchId && b.ItemId == mi.ItemId);
            if (bmi?.IsAvailable != true) continue;
            decimal price = bmi.PriceOverride ?? mi.BasePrice;
            string addOns = mi.AddOns.Any() ? string.Join(", ", mi.AddOns.Select(a => a.Name)) : "—";
            Console.WriteLine($"   {mi.ItemId,-5} {mi.Name,-22} {mi.Category,-14} {price,-10:C} {addOns}");
        }
    }
}
