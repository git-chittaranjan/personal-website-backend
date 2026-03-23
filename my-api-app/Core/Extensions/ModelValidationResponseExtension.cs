using Microsoft.AspNetCore.Mvc;
using my_api_app.Core.Responses;
using my_api_app.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Reflection.Metadata;

namespace my_api_app.Core.Extensions
{
    /// <summary>
    /// Request → Model Binding → FluentValidation Runs → ModelState Invalid → Below InvalidModelStateResponseFactory Fires
    /// → 400 returned → Controller never reached & no exception thrown (because we are handling and returning a response instead of throwing an exception)
    /// → GlobalExceptionMiddleware NOT triggered
    /// </summary>
    public static class ModelValidationResponseExtension
    {
        public static IServiceCollection AddCustomModelValidationResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new ValidationErrorDto
                        {
                            Field = kvp.Key,
                            Error = e.ErrorMessage
                        }))
                        .ToList();

                    //ILoggerFactory() is used because ModelValidationResponseExtension is a static class like Program.cs, ILogger<T> DI cannot be injected here.
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ModelValidation");
                    var userId = context.HttpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

                    logger.LogWarning(
                        "Model Validation failed | Path:{Path} | User:{UserId} | Fields:{Fields}",
                        context.HttpContext.Request.Path, userId, string.Join(", ", errors.Select(e => $"{e.Field}='{e.Error}'"))   // Fields:pan_number='PAN number is required.', annual_income='Must be greater than 0.'
                    );

                    // Call the IApiResponseFactory (Every HttpContext contains a scoped DI container in RequestServices.
                    var responseFactory = context.HttpContext.RequestServices.GetRequiredService<IApiResponseFactory>();

                    var response = responseFactory.Failure(Statuses.ValidationFailed, errors);

                    return new BadRequestObjectResult(response)
                    {
                        ContentTypes = { "application/json" }
                    };
                };
            });

            return services;
        }
    }
}
