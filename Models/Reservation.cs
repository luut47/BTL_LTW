namespace BTL_LTW.Models
{
    public class Reservation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Email { get; set; } = "";
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public int People { get; set; } = 2;
        public string? Note { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 

        public string? AssignedTable { get; set; }
        public string Status { set; get; } = "Pending";
        public string? LinkOrderId { get; set; }
    }
}
