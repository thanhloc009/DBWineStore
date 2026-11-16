using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBWineStore.Models
{
    public class AdminOrderViewModel
    {
    }
    
    public class StatusOption
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    // One row in the admin order table.
    public class AdminOrderRowViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string RecipientName { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethodName { get; set; }
        public int? PaymentStatusID { get; set; }
        public int? ShippingStatusID { get; set; }
    }

    // Admin list page (table + filters).
    public class AdminOrderListViewModel
    {
        public List<AdminOrderRowViewModel> Orders { get; set; }
        public List<StatusOption> PaymentStatuses { get; set; }
        public List<StatusOption> ShippingStatuses { get; set; }

        // current filters
        public string Keyword { get; set; }
        public int? PaymentFilter { get; set; }
        public int? ShippingFilter { get; set; }

        // little stats shown on top
        public int TotalOrders { get; set; }
        public int WaitingOrders { get; set; }      // shipping = PENDING
        public int UnpaidOrders { get; set; }       // payment  = PENDING
    }

    // Admin detail page of one order.
    public class AdminOrderDetailsViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string RecipientName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethodName { get; set; }
        public int? PaymentStatusID { get; set; }
        public int? ShippingStatusID { get; set; }

        public List<OrderItemViewModel> Items { get; set; }
        public List<StatusOption> PaymentStatuses { get; set; }
        public List<StatusOption> ShippingStatuses { get; set; }
    }
}