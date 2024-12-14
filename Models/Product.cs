namespace OnlinePaymentSystem.Models
{

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
