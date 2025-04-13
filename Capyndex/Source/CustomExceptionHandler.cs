using System.Net;
using Capyndex.Shared.Guards;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Capyndex.Infrastructure;

public static class CustomExceptionHandlerExtensions
{
    private static readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers = new()
    {
        { typeof(NotFoundException), HandleNotFoundExceptionAsync },
        { typeof(ValidationException), HandleValidationExceptionAsync },
        { typeof(BadHttpRequestException), HandleBadRequestExceptionAsync },
        { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessExceptionAsync }
    };

    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app, ILogger? logger = null)
    {
        app.UseExceptionHandler(
            errApp =>
            {
                errApp.Run(
                    async ctx =>
                    {
                        var exHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();

                        if (exHandlerFeature is not null)
                        {
                            logger ??= ctx.RequestServices.GetService<ILogger<CustomExceptionHandler>>();
                            var route = exHandlerFeature.Endpoint?.DisplayName?.Split(" => ")[0];
                            var exception = exHandlerFeature.Error;
                            var exceptionType = exception.GetType();

                            // Log the exception
                            logger?.LogError(
                                exception,
                                "Exception {ExceptionType} occurred at {Route} with reason: {Reason}",
                                exceptionType.Name,
                                route,
                                exception.Message);

                            // Set content type for all responses
                            ctx.Response.ContentType = "application/problem+json";

                            // Find a handler for this exception type
                            bool handled = false;

                            foreach (var (type, handler) in _exceptionHandlers)
                            {
                                if (type.IsAssignableFrom(exceptionType))
                                {
                                    await handler(ctx, exception);
                                    handled = true;

                                    break;
                                }
                            }

                            // If no specific handler was found, use default handler
                            if (!handled)
                            {
                                await HandleUnknownExceptionAsync(ctx, exception);
                            }
                        }
                    });
            });

        return app;
    }

    private static async Task HandleNotFoundExceptionAsync(HttpContext ctx, Exception ex)
    {
        var notFoundEx = (NotFoundException)ex;
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;

        await ctx.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Resource Not Found",
                Detail = notFoundEx.Message,
                Instance = ctx.Request.Path,
                Extensions =
                {
                    ["key"] = notFoundEx.Key,
                    ["parameterName"] = notFoundEx.ParameterName
                }
            });
    }

    private static async Task HandleValidationExceptionAsync(HttpContext ctx, Exception ex)
    {
        var validationEx = (ValidationException)ex;
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;

        await ctx.Response.WriteAsJsonAsync(
            new ValidationProblemDetails(
                new Dictionary<string, string[]> { { "Validation", new[] { validationEx.Message } } })
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Failed",
                Detail = validationEx.Message,
                Instance = ctx.Request.Path
            });
    }

    private static async Task HandleBadRequestExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;

        await ctx.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Detail = ex.Message,
                Instance = ctx.Request.Path
            });
    }

    private static async Task HandleUnauthorizedAccessExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await ctx.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Detail = ex.Message,
                Instance = ctx.Request.Path
            });
    }

    private static async Task HandleUnknownExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await ctx.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Detail = "An unexpected error has occurred",
                Instance = ctx.Request.Path
            });
    }
}

// Simple class to support the logger dependency
public class CustomExceptionHandler { }