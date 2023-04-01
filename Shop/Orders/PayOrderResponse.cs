namespace Shop.Orders;

public record class PayOrderResponse(Guid OrderId, string PaymentUrl);