using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace MedControl.Api.Extensions;

public static class ExceptionHandlerExtensions
{
    public static WebApplication UseApiExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionFeature is null)
                {
                    return;
                }

                if (exceptionFeature.Error is ValidationException validationException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/problem+json";

                    var errors = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());

                    await Results.ValidationProblem(errors).ExecuteAsync(context);
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await Results.Problem(
                    title: "An unexpected error occurred.",
                    statusCode: StatusCodes.Status500InternalServerError)
                    .ExecuteAsync(context);
            });
        });

        return app;
    }
}
