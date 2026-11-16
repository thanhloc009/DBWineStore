using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DBWineStore.Models
{
    // One radio option, loaded from dbo.PaymentMethod.
    public class PaymentMethodOption
    {
        public string Code { get; set; }   // PaymentMethod.Code  (COD, BANK)
        public string Name { get; set; }   // PaymentMethod.MethodName
    }

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        public string Note { get; set; }

        // Holds the PaymentMethod.Code the customer picked (COD or BANK).
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        // Filled by the controller from the database; not posted back.
        public List<PaymentMethodOption> PaymentMethods { get; set; }

        public string CouponCode { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal FinalAmount { get; set; }
    }
}