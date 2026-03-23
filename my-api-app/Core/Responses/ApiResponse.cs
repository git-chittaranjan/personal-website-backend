using System.Text.Json.Serialization;

namespace my_api_app.Core.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string? StatusCode { get; set; } = null;
        public T? Data { get; set; } = default;
        public string? Description { get; set; } = null;
        public object? Errors { get; set; } = null;
        public string? TraceId { get; set; } = null;
        public Pagination? Pagination { get; set; }
}

    public class Pagination
    {
        public int TotalCount { get; set; } = 1; //TotalCount: Total number of records available in the database
        public int PageNumber { get; set; } = 1; //PageNumber: Current page number being requested
        public int PageSize { get; set; } = 1; //PageSize: Number of records to be returned per page
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); //TotalPages: Total number of pages calculated
    }
}
