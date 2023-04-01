namespace Orders.Payments;

public record class PaymentRequest(Guid OrderId, decimal Amount, string Description);