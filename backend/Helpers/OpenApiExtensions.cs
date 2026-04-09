using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace RestaurantManagement.Api.Helpers;

public static class OpenApiExtensions
{
    public static IEndpointRouteBuilder MapSwaggerDocumentation(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/swagger/v1/swagger.json", (IActionDescriptorCollectionProvider provider) =>
        {
            var paths = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            foreach (var action in provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>())
            {
                var routeTemplate = action.AttributeRouteInfo?.Template;
                if (string.IsNullOrWhiteSpace(routeTemplate))
                {
                    continue;
                }

                var normalizedRoute = "/" + routeTemplate.Trim('/');
                if (!paths.TryGetValue(normalizedRoute, out var operations))
                {
                    operations = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    paths[normalizedRoute] = operations;
                }

                var httpMethods = action.MethodInfo.GetCustomAttributes(true)
                    .OfType<HttpMethodAttribute>()
                    .SelectMany(attribute => attribute.HttpMethods)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .DefaultIfEmpty("GET");

                var allowAnonymous = action.MethodInfo.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() is not null ||
                    action.ControllerTypeInfo.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() is not null;

                foreach (var method in httpMethods)
                {
                    var responses = new Dictionary<string, object>
                    {
                        ["200"] = new { description = "Success" },
                        ["400"] = new { description = "Bad Request" },
                        ["401"] = new { description = "Unauthorized" },
                        ["403"] = new { description = "Forbidden" }
                    };

                    operations[method.ToLowerInvariant()] = new
                    {
                        tags = new[] { action.ControllerName },
                        summary = $"{action.ControllerName} - {action.ActionName}",
                        responses,
                        security = allowAnonymous
                            ? Array.Empty<object>()
                            : new[] { new Dictionary<string, string[]> { ["Bearer"] = Array.Empty<string>() } }
                    };
                }
            }

            var document = new
            {
                openapi = "3.0.1",
                info = new
                {
                    title = "Khan Bahadur Restora API",
                    version = "v1",
                    description = "Authentication, menu, orders, payments, inventory, dashboards, and reports."
                },
                servers = new[] { new { url = "/" } },
                components = new
                {
                    securitySchemes = new
                    {
                        Bearer = new
                        {
                            type = "http",
                            scheme = "bearer",
                            bearerFormat = "JWT",
                            description = "Use the access token from POST /api/auth/login."
                        }
                    }
                },
                paths
            };

            return Results.Json(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }).ExcludeFromDescription();

        endpoints.MapGet("/swagger", async context =>
        {
            const string html = """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Khan Bahadur Restora API</title>
  <style>
    body { font-family: Arial, sans-serif; margin: 0; background: #f7f4ef; color: #1d1d1d; }
    header { background: #14213d; color: white; padding: 24px; }
    main { max-width: 1080px; margin: 0 auto; padding: 24px; }
    .card { background: white; border-radius: 16px; box-shadow: 0 12px 30px rgba(0,0,0,0.08); padding: 24px; margin-bottom: 20px; }
    code { background: #f1f1f1; border-radius: 6px; padding: 2px 6px; }
    pre { white-space: pre-wrap; word-break: break-word; }
  </style>
</head>
<body>
  <header>
    <h1>Khan Bahadur Restora API</h1>
    <p>JSON docs: <a href="/swagger/v1/swagger.json" style="color:white">/swagger/v1/swagger.json</a></p>
  </header>
  <main>
    <div class="card">
      <p>Authenticate with <code>POST /api/auth/login</code>, then send <code>Authorization: Bearer &lt;token&gt;</code>.</p>
    </div>
    <div class="card">
      <h2>Paths</h2>
      <pre id="paths">Loading...</pre>
    </div>
  </main>
  <script>
    fetch('/swagger/v1/swagger.json')
      .then(response => response.json())
      .then(doc => document.getElementById('paths').textContent = JSON.stringify(doc.paths, null, 2))
      .catch(() => document.getElementById('paths').textContent = 'Unable to load documentation.');
  </script>
</body>
</html>
""";

            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(html);
        }).ExcludeFromDescription();

        return endpoints;
    }
}
