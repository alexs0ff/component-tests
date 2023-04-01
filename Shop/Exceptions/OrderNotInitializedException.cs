namespace Shop.Exceptions;

public class OrderNotInitializedException : Exception
{
    public Guid BasketId { get; }
    public OrderNotInitializedException(string? message, Guid basketId) : base(message)
    {
        BasketId = basketId;
    }
}