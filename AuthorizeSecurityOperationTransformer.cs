using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CarwashApi;

internal sealed class AuthorizeSecurityOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
      if (context.Document is null)
            return Task.CompletedTask;
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        // Explicit anonymous — do not require JWT in OpenAPI for this operation
        if (metadata.OfType<AllowAnonymousAttribute>().Any())
            return Task.CompletedTask;
        // Requires auth — attach Bearer security requirement for Swagger / OpenAPI
        if (metadata.OfType<AuthorizeAttribute>().Any())
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, context.Document)] = [],
            });
        }
        return Task.CompletedTask;
    }
}