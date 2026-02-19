namespace my_api_app.DTOs
{
    public class ValidationErrorDto
    {
        public string? Field { get; set; } = default!;
        public string? Error { get; set; } = default!;
    }
}
