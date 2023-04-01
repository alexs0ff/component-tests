using System.Collections.Concurrent;

namespace Orders.Payments;

public class PaymentService
{
    private static readonly ConcurrentDictionary<Guid, PaymentStatus> _paymentStatusMap = new();

    public Task<PaymentResponse> CreatePayment(PaymentRequest request)
    {
        var status = PaymentStatus.New;
        status = _paymentStatusMap.GetOrAdd(request.OrderId, status);
        return Task.FromResult(new PaymentResponse(status));
    }

    public Task<PaymentDto?> Get(Guid orderId)
    {
        PaymentDto? result = null;
        if (_paymentStatusMap.TryGetValue(orderId, out var paymentstatus))
            result = new PaymentDto(orderId, paymentstatus);
        return Task.FromResult(result);
    }

    public Task<PaymentDto?> SetStatus(Guid orderld, PaymentStatus newStatus)
    {
        PaymentDto? result = null;
        if (_paymentStatusMap.TryGetValue(orderld, out var currentstatus))
            if (_paymentStatusMap.TryUpdate(orderld, newStatus, currentstatus))
                result = new PaymentDto(orderld, newStatus);
        return Task.FromResult(result);
    }
}