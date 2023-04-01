namespace Orders.Payments;

public record class PaymentDto(Guid OrderId, PaymentStatus Status);