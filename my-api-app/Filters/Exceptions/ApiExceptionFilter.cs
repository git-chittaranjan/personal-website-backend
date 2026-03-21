using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using my_api_app.Exceptions.BusinessExceptions;
using my_api_app.Responses;

namespace my_api_app.Filters.Exceptions
{
    //Not used anywhere since Global Exception Middleware already handles all the scenarios
    //Not registered in Program.cs
    //No need to implement snak_case, since that option is added to Controller in Program.cs
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly IApiResponseFactory _responseFactory;

        public ApiExceptionFilter(IApiResponseFactory responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public void OnException(ExceptionContext context)
        {
            var status = context.Exception switch
            {
                BusinessException be => be.Status,
                ArgumentException ae => Statuses.BadRequest,
                _ => Statuses.InternalServerError
            };

            context.Result = new ObjectResult(_responseFactory.Failure(status))
            {
                StatusCode = status.HttpCode
            };

            // Mark exception as handled — stops it bubbling further
            context.ExceptionHandled = true;
        }
    }
}
