using OOP_Project_2.Data;
using OOP_Project_2.Enums;
using OOP_Project_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project_2.Services
{
    public static class OrderServices
    {
        public static (bool Success, string Message)
            PlaceOrder(int employeeId, int customerId, int branchId, OrderType orderType, string? deliveryAddress,
            List<(int ItemId, int Qty, string? notes, List<int> AddOnIds)> items)

        {
            //Only Waiters and Cashiers can place orders
            var employee = Database.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
            if (employee is null)
                return (false, "Employee not found");
            if (employee is not Waiter && employee is not Cashier)
                return (false, "Only Waiters and Cashiers can place orders");

            //Employee must belong to same branch as order
            if (!employee.AssignmedBranchIds.Contains(branchId))
                return (false, "Employee does not belong to the specified branch");

            var customer = Database.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer is null)
                return (false, "Customer not found");

            var branch = Database.Branches.FirstOrDefault(b => b.BranchId == branchId);
            if (branch is null)
                return (false, "Branch not found");

            //Delivery orders require delivery address
            if (orderType == OrderType.Delivery && string.IsNullOrWhiteSpace(deliveryAddress))
                return (false, "Delivery orders require a delivery address");
            //Order must have at least one item
            if (items is null || items.Count == 0)
                return (false, "Order must have at least one item");



            var orderItems = new List<OrderItem>();
            int itemSeq = Database.NextOrderItemId;
            foreach (var (ItemId, Qty, notes, AddOnIds) in items)
            {

                if (Qty <= 0)
                    return (false, $"Item quantity must be greater than zero for ItemId {ItemId}");

                //Menu item must be available at branch
                var branchItem = Database.BranchMenuItems.FirstOrDefault(bi => bi.BranchId == branchId && bi.ItemId == ItemId);
                if (branchItem is null || !branchItem.IsAvailable)
                    return (false, $"ItemId {ItemId} is not available at the specified branch");

                var menuItem = Database.MenuItems.FirstOrDefault(mi => mi.ItemId == ItemId);
                if (menuItem is null)
                    return (false, $"ItemId {ItemId} not found in menu");


                decimal unitPrice = Database.BranchMenuItems.FirstOrDefault(bi => bi.BranchId == branchId && bi.ItemId == ItemId)?.PriceOverride ?? menuItem.BasePrice;

                foreach (var addOnId in AddOnIds)
                {
                    var addOn = menuItem.AddOns.FirstOrDefault(a => a.AddOnId == addOnId);
                    if (addOn is null)
                        return (false, $"AddOnId {addOnId} not found for ItemId {ItemId}");
                    unitPrice += addOn.ExtraPrice;
                }


                orderItems.Add(new OrderItem
                {
                    OrderItemId = itemSeq++,
                    ItemId = ItemId,
                    Quantity = Qty,
                    UnitPrice = unitPrice,
                    SpecialNotes = notes,
                    SelectedAddOnIds = AddOnIds

                });


            }


            var order = new Order
            {
                OrderId = Database.NextOrderId++,
                OrderType = orderType,
                BranchId = branchId,
                CustomerId = customerId,
                DateTime = DateTime.UtcNow,
                DeliveryAddress = deliveryAddress,
                HandledByEmployeeId = employeeId,
                Status = OrderStatus.Pending,
                Items = orderItems,
                TotalAmount = orderItems.Sum(x => x.UnitPrice * x.Quantity),
            };

            Database.NextOrderItemId = itemSeq;
            Database.Orders.Add(order);

            if (orderType == OrderType.Delivery)
            {
                Database.Deliveries.Add(new Delivery
                {
                    DeliveryId = Database.NextDeliveryId++,
                    OrderId = order.OrderId,
                    DeliveryAddress = deliveryAddress,
                    Status = DeliveyStatus.AwaitingPickup
                });
            }
            return (true, $"Order placed successfully with OrderId {order.OrderId}");
        }


        public static (bool Success, string Message) StartPreparing(int orderId, int ChefId)
        {
            var order = Database.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order is null)
                return (false, "Order not found");
            if (order.Status != OrderStatus.Pending)
                return (false, "Only pending orders can be started");

            var chef = Database.Employees.FirstOrDefault(e => e.EmployeeId == ChefId);
            if (chef is null || chef is not Chef)
                return (false, "Chef not found");

            if (!chef.AssignmedBranchIds.Contains(order.BranchId))
                return (false, "Chef does not belong to the same branch as the order");

            bool isSufficient = InventoryServices.IsSufficient(order.BranchId, order.Items);
            if (!isSufficient)
            {
                var shortfalls = InventoryServices.GetShortfalls(order.BranchId, order.Items);
                string shortfallMsg = string.Join(", ", shortfalls.Select(s => $"IngredientId {s.Key}: Short by {s.Value}"));
                return (false, $"Insufficient inventory to start preparing the order. Shortfalls: {shortfallMsg}");
            }
            InventoryServices.Deduct(order.BranchId, order.Items);
            order.Status = OrderStatus.InProgress;
            return (true, "Order preparation started successfully");


        }


        public static (bool Success, string Message) ServeOrder(int orderId, int ChefId)
        {
            var order = Database.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order is null)
                return (false, "Order not found");
            if (order.Status != OrderStatus.InProgress)
                return (false, "Only orders in progress can be served");
            var employee = Database.Employees.FirstOrDefault(e => e.EmployeeId == ChefId);
            if (employee is null)
                return (false, "Employee not found");
            if (employee is not Waiter)
                return (false, "Only Waiters can serve orders");
            if (!employee.AssignmedBranchIds.Contains(order.BranchId))
                return (false, "Employee does not belong to the same branch as the order");
            order.Status = OrderStatus.Served;
            return (true, "Order served successfully");
        }

        public static (bool Success, string Message) ProcessPayment(int orderId, int CashierId, PaymentMethod method)
        {
            var order = Database.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order is null)
                return (false, "Order not found");
            if (order.Status != OrderStatus.Served)
                return (false, "Only served orders can be paid for");
            var employee = Database.Employees.FirstOrDefault(e => e.EmployeeId == CashierId);
            if (employee is null)
                return (false, "Employee not found");
            if (employee is not Cashier)
                return (false, "Only Cashiers can process payments");
            if (!employee.AssignmedBranchIds.Contains(order.BranchId))
                return (false, "Employee does not belong to the same branch as the order");
            order.PaymentMethod = method;
            order.Status = OrderStatus.Completed;
            return (true, "Payment processed successfully");

            var customer = Database.Customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);
            int earnedPoints = (int)Math.Floor(order.TotalAmount);
            if (customer is not null)
            {
                customer.LoyaltyPoints += earnedPoints;
            }
            return (true, $"Payment processed successfully. Customer earned {earnedPoints} loyalty points.");
        }


        public static (bool Success, string Message) CancelOrder(int orderId, int EmployeeId)
        {
            var order = Database.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order is null)
                return (false, "Order not found");
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
                return (false, "Cannot cancel a completed or already cancelled order");
            var employee = Database.Employees.FirstOrDefault(e => e.EmployeeId == EmployeeId);
            if (employee is null)
                return (false, "Employee not found");
            if (!employee.AssignmedBranchIds.Contains(order.BranchId))
                return (false, "Employee does not belong to the same branch as the order");
            if (employee is Waiter || employee is Cashier )
            {
                if (order.Status == OrderStatus.InProgress)
                    return (false, "Waiters and Cashiers can cancel only pending orders");


            }else if(employee is BranchManger)
                    {
                if (order.OrderType == OrderType.Delivery)
                {
                    var delivery = Database.Deliveries.FirstOrDefault(d => d.OrderId == order.OrderId);
                    if (delivery is not null)
                    {
                        delivery.Status = DeliveyStatus.onTheWay;
                        return (false, " Branch Managers cannot cancel delivery orders that are already on the way");
                    }
                }


                    }
            order.Status = OrderStatus.Cancelled;
            return (true, "Order cancelled successfully");


        }
    }
}