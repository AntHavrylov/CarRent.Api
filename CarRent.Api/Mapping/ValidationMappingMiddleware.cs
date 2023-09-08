using CarRent.Contracts.Responses;
using FluentValidation;

namespace CarRent.Api.Mapping
{
    public class ValidationMappingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationMappingMiddleware> _logger;

        public ValidationMappingMiddleware(RequestDelegate next, 
            ILogger<ValidationMappingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) 
        {
            try 
            {
                await _next(context);   
            }
            catch(ValidationException ex)
            {
                context.Response.StatusCode = 400;
                var validationFailureResponse = new ValidationFailureResponse
                {
                    Errors = ex.Errors.Select(x => new ValidationResponse
                    {
                        Message = x.ErrorMessage,
                        PropertyName = x.PropertyName,
                    })
                };
                foreach(var error in ex.Errors) 
                {
                    _logger.LogError(ex, "Property {{PropertyName}} validation failed", error.PropertyName);
                }
                await context.Response.WriteAsJsonAsync(validationFailureResponse);
            }

        }
    }
}
