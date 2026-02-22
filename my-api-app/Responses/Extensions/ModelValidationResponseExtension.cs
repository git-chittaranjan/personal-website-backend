using Microsoft.AspNetCore.Mvc;
using my_api_app.DTOs;

namespace my_api_app.Responses.Extensions
{
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
