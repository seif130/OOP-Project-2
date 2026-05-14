namespace OOP_Project_2.UI;

public static class MainMenu
{
    public static void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║         RESTAURANT MANAGEMENT SYSTEM                 ║");
            Console.WriteLine("  ║                  Main Menu                           ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            ConsoleHelper.PrintOption("1", "Branch Management");
            ConsoleHelper.PrintOption("2", "Employee Management");
            ConsoleHelper.PrintOption("3", "Customer Management");
            ConsoleHelper.PrintOption("4", "Menu Management");
            ConsoleHelper.PrintOption("5", "Order Management");
            ConsoleHelper.PrintOption("6", "Delivery Management");
            ConsoleHelper.PrintOption("7", "Inventory Management");
            ConsoleHelper.PrintOption("8", "Feedback");
            ConsoleHelper.PrintOption("9", "Reports & Analytics");
            Console.WriteLine();
            ConsoleHelper.PrintOption("0", "Exit");

            var choice = ConsoleHelper.ReadString("Choice");
            switch (choice)
            {
                case "1": BranchUI.Run();    break;
                case "2": EmployeeUI.Run();  break;
                case "3": CustomerUI.Run();  break;
                case "4": MenuUI.Run();      break;
                case "5": OrderUI.Run();     break;
                case "6": DeliveryUI.Run();  break;
                case "7": InventoryUI.Run(); break;
                case "8": FeedbackUI.Run();  break;
                case "9": ReportsUI.Run();   break;
                case "0":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n  Goodbye!\n");
                    Console.ResetColor();
                    return;
                default:
                    ConsoleHelper.Error("Invalid option. Please choose 0–9.");
                    ConsoleHelper.Wait();
                    break;
            }
        }
    }
}
