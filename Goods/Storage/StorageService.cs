namespace Goods.Storage;

public class StorageService
{
    public Task<ReservationResponse> Reserve(ReservationRequest request)
    {
        return Task.FromResult(new ReservationResponse(request.Items.Select(
            i => new ProductDto(i.Name, i.Count, Random.Shared.Next(10,100))
        ).ToArray()));
    }

    public Task<ReleaseResponse> Release(ReleaseRequest request)
    {
        return Task.FromResult(new ReleaseResponse(request.Items.Sum(i => i.Count)));
    }
}