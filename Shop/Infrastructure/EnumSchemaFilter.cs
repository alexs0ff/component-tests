using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shop.Infrastructure;

public class EnumSchemaFilter : ISchemaFilter
{
    private const string NAME = "x-enumNames";

    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        var typeinfo = context.Type;
        if (typeinfo.IsEnum && !model.Extensions.ContainsKey(NAME))
        {
            var names = Enum.GetNames(context.Type);
            var arr = new OpenApiArray();
            arr.AddRange(names.Select(name => new OpenApiString(name)));
            model.Extensions.Add(NAME, arr);
        }
    }
}