using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CarwashApi;

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer 
{
    public async Task TransformAsync(
        OpenApiDocument document, 
        OpenApiDocumentTransformerContext context, 
        CancellationToken cancellationToken)
    {
        var scheme = await authenticationSchemeProvider.GetAllSchemesAsync();
        if(!scheme.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme)) return;

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            [JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Description = "JWT Authorization: Bearer {token}",
            },
        };

        // if(document.Paths is null) return;

        // foreach(var path in document.Paths.Values) 
        // {
        //     if(path.Operations is null) continue;
        //     foreach (var operation in path.Operations.Values) 
        //     {
        //         operation.Security ??= [];
        //         operation.Security.Add(new OpenApiSecurityRequirement 
        //         {
        //             [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = [],
        //         });
        //     }
        // }
    }
    
}