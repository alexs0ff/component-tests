namespace Shop.Entities;

    public class Basket
    {
        public Guid Id { get; set; }
        public string PromoCode { get; set; } = string.Empty;   
        public Guid Userid { get; set; }
        public User User { get; set; } = default!;
        public decimal Total { get; set; }
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
