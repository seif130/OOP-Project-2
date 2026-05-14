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
    public static class DeliveryService
    {
        public static (bool sucess, string message) AssignStaff(int orderId, int staffId, int assingedbyEmployeeId)
        {
            var order = Database.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return (false, "Order not found");
            if (order.OrderType != OrderType.Delivery)
                return (false, "Order is not for delivery");
            if (order.Status != OrderStatus.Served)
                return (false, "Delivery staff can only be assigned to served orders");
            var staff = Database.Employees.FirstOrDefault(e => e.EmployeeId == staffId);
            if (staff == null)
                return (false, "Invalid staff");
            if (staff is not Waiter && staff is not Cashier && staff is not BranchManger)
                return (false, "only waiters or cashiers or branch manger can assign delivery staff");
            if (!staff.AssignmedBranchIds.Contains(order.BranchId))
                return (false, "Staff is not assigned to the branch of the order");

            var stafflist = Database.DeliveryStaffList.FirstOrDefault(x => x.StaffId == staffId);
            if (stafflist == null) return (false, "not found in delivery staff list");
            if (!stafflist.IasAvailable)
                return (false, "Staff is already assigned to another delivery");

            if (stafflist.BranchId != order.BranchId)
                return (false, "delivery staff must be from the same branch as the order");

            var delivery = Database.Deliveries.FirstOrDefault(d => d.OrderId == orderId);
            if (delivery == null)
                return (false, "Delivery not found");
            delivery.DeliveryStaffId = staffId;
            delivery.Status = DeliveyStatus.onTheWay;
            stafflist.IasAvailable = false;
            return (true, "On the way......................");
        }



        public static (bool sucess, string message) MarkDeliverd(int deliveryId, int deliverystaffId)
        {
            var delivery = Database.Deliveries.FirstOrDefault(d => d.DeliveryId == deliveryId);
            if (delivery == null)
                return (false, "Delivery not found");
            if (delivery.Status != DeliveyStatus.onTheWay)
                return (false, "Only deliveries that are on the way can be marked as delivered");
            if (delivery.DeliveryStaffId != deliverystaffId)
                return (false, "Not your delivery");

            delivery.Status = DeliveyStatus.Delivered;

            delivery.DateTime = DateTime.Now;
            var order = Database.Orders.FirstOrDefault(o => o.OrderId == delivery.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Completed;
                var customer = Database.Customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);
                if (customer != null)
                {
                  customer.LoyaltyPoints += (int)Math.Floor(order.TotalAmount);
                }
            }
            var stafflist = Database.DeliveryStaffList.FirstOrDefault(x => x.StaffId == deliverystaffId);
            if (stafflist != null) 
                stafflist.IasAvailable = true;

            return (true, "Delivery marked as delivered");
        }

        public static (bool sucess, string message) MarkFailed(int deliveryId, int deliverystaffId, string reason)
        {
            var delivery = Database.Deliveries.FirstOrDefault(d => d.DeliveryId == deliveryId);
            if (delivery == null)
                return (false, "Delivery not found");
            if (delivery.DeliveryStaffId != deliverystaffId)
                return (false, "Not your delivery");

            if (string.IsNullOrWhiteSpace(reason))
                return (false, "Failure reason must be provided");
          
            delivery.Status = DeliveyStatus.Failed;
            delivery.FailureReason = reason;

         var order = Database.Orders.FirstOrDefault(o => o.OrderId == delivery.OrderId);
            if (order != null)
                order.Status = OrderStatus.Cancelled;
            return (true, $"Delivery failed: {reason}");

        }
    }
}