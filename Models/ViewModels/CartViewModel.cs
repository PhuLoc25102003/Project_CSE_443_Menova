using System.Collections.Generic;
namespace Menova.Models.ViewModels
{
    public class CartViewModel
    {
        public Cart Cart { get; set; }
        public List<CartItemViewModel> Items  { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total => Subtotal + ShippingFee;
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int VariantId { get; set; }
        public string SizeName  { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }
}
