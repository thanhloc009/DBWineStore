using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBWineStore.Models
{
    public class OrderViewModels
    {
    }
    public class OrderSummaryViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public int ItemCount { get; set; }
        public decimal Total { get; set; }

        public string PaymentStatusName { get; set; }
        public string PaymentStatusCode { get; set; }
        public string ShippingStatusName { get; set; }
        public string ShippingStatusCode { get; set; }
    }

    // One product line inside an order.
    public class OrderItemViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }   // file name only, stored in Content/images
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get { return UnitPrice * Quantity; } }
    }

    // The full order shown on the customer's details page.
    public class OrderDetailsViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string RecipientName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }

        public string PaymentMethodName { get; set; }
        public string PaymentStatusName { get; set; }
        public string PaymentStatusCode { get; set; }
        public string ShippingStatusName { get; set; }
        public string ShippingStatusCode { get; set; }

        public List<OrderItemViewModel> Items { get; set; }
    }
}