namespace my_api_app.Core.Responses
{
    public class ValidationErrorDto
    {
        public string? Field { get; set; } = default!;
        public string? Error { get; set; } = default!;
    }
}
