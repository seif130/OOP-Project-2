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
    public class FeedbackService
    {

        public static (bool success, string message) SubmitFeedback(int orderId, int customerId, int rating, string comment)
        {
            var order = Database.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return (false, "Order not found");
            if (order.CustomerId != customerId)
                return (false, "You can only submit feedback for your own orders");
            if (order.Status != OrderStatus.Completed)
                return (false, "You can only submit feedback for completed orders");
            bool submitbefore = Database.Feedbacks.Any(f => f.OrderId == orderId && f.CustomerId == customerId);
            if (submitbefore)
                return (false, "You have already submitted feedback for this order");
            if (rating < 1 || rating > 5)
                return (false, "Rating must be between 1 and 5");
            var feedback = new Feedback
            {
                FeedbackId = Database.Feedbacks.Count + 1,
                OrderId = orderId,
                CustomerId = customerId,
                Rating = rating,
                Comments = comment,
                DateTime = DateTime.Now
            };
            Database.Feedbacks.Add(feedback);
            return (true, "Feedback submitted successfully");
        }
    }
}
