namespace my_api_app.Responses
{
    public class ApiStatus
    {
        public int HttpCode { get; set; }
        public string Code { get; set; } = default!;
        public string Message { get; set; } = default!;
    }
}
