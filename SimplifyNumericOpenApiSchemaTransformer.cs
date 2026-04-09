using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CarwashApi;

/// <summary>
/// .NET OpenAPI can emit int32 as integer|string unions with a pattern; Swagger UI breaks on those for path params.
/// </summary>
internal sealed class SimplifyNumericOpenApiSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (string.Equals(schema.Format, "int32", StringComparison.OrdinalIgnoreCase))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Format = "int32";
            schema.Pattern = null;
            schema.AnyOf?.Clear();
            schema.OneOf?.Clear();
            schema.AllOf?.Clear();
        }
        else if (string.Equals(schema.Format, "double", StringComparison.OrdinalIgnoreCase))
        {
            schema.Type = JsonSchemaType.Number;
            schema.Format = "double";
            schema.Pattern = null;
            schema.AnyOf?.Clear();
            schema.OneOf?.Clear();
            schema.AllOf?.Clear();
        }

        return Task.CompletedTask;
    }
}