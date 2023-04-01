namespace Shop.Exceptions;

public class EntityNotFoundException : Exception
{
    public string EntityName { get; }
    public EntityNotFoundException(string? message, string entityName) : base(message)
    {
        EntityName = entityName;
    }
}