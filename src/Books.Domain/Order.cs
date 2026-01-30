namespace Books.Domain;

public class Order
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum OrderStatus
{
    Pending,
    Completed,
    Cancelled
}
