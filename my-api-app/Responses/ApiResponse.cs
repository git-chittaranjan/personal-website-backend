using System.Text.Json.Serialization;

namespace my_api_app.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string? StatusCode { get; set; } = null;
        public T? Data { get; set; } = default;
        public string? Description { get; set; } = null;
        public string? Errors { get; set; } = null;
        public string? TraceId { get; set; } = null;
        public Pagination? Pagination { get; set; }
}

    public class Pagination
    {
        public int Total { get; set; } = 1;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 1;
    }
}
