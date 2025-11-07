namespace BTL_LTW.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int MenuItemId { get; set; }
        public string? MenuItemName { get; set; }
        public int Qty { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = "Pending";// Pending, Preparing, Ready, Served

    }
}
