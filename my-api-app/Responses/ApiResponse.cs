using System.Text.Json.Serialization;

namespace my_api_app.Responses
{
    public class ApiResponse<T>
    {
        public int? StatusCode { get; set; } = default;

        public T? Data { get; set; } = default;

        public string? Description { get; set; } = null;

        public string? Errors { get; set; } = null;

        public int Total { get; set; } = 1;

        public int Page { get; set; } = 1; 
    }
}
