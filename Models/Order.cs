namespace BTL_LTW.Models
{
    public class Order
    {
        public string Id { get; set; }
        public string? TableToken { get; set; }
        public string? OrderToken { get; set; }
        public string? AssignedTable { get; set; }
        public bool IsTakeAway { get; set; } = false;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public decimal Total { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Preparing, Ready, Served, Completed
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? PaymenMethod { get; set; }
        public string? ReservationId { get; set; }
        public bool IsPaid { get; set; } = false;
        //public string? BankPaymentLocation { get; set; }
    }
}
