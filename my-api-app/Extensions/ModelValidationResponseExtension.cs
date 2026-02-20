using Microsoft.AspNetCore.Mvc;
using my_api_app.DTOs;
using my_api_app.Helpers;
using my_api_app.Responses;

namespace my_api_app.Extensions
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

                    var response = ApiResponseFactory.Failure(Statuses.ValidationFailed, errors);

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
