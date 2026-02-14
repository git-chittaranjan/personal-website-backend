namespace my_api_app.Responses
{
    public class ApiStatus
    {
        public int HttpCode { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}
