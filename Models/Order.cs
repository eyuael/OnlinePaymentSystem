namespace OnlinePaymentSystem.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<OrderItem> Items { get; set; } = new();
    }
}
