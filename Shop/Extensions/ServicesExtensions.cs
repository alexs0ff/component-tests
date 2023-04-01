using Shop.Exceptions;

namespace Shop.Extensions;

public static class ServicesExtensions
{
    public static async Task<IResult> ResultOnExceptionAsync<TService, TOutput>(this TService service,
        Func<Task<TOutput>> funct) where TService : class
    {
        TOutput result;
        try
        {
            result = await funct();
        }
        catch (EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (OrderNotInitializedException)
        {
            return Results.UnprocessableEntity();
        }

        return Results.Ok(result);
    }
}