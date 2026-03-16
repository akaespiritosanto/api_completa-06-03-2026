using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace criacao_api4.Filters;

public sealed class ApiKeyFilter : IAsyncActionFilter
{
    private const string HeaderName = "X-API-KEY";
    private const string DefaultApiKey = "scott";
    private readonly string? _expectedApiKey;

    public ApiKeyFilter()
    {
        _expectedApiKey = DefaultApiKey;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (string.IsNullOrWhiteSpace(_expectedApiKey))
        {
            context.Result = new ObjectResult("Server API key is not configured.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedApiKey) || string.IsNullOrWhiteSpace(providedApiKey))
        {
            context.Result = new UnauthorizedObjectResult($"Missing required header '{HeaderName}'.");
            return;
        }

        if (!string.Equals(providedApiKey.ToString(), _expectedApiKey, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult("Invalid API key.");
            return;
        }

        await next();
    }
}
