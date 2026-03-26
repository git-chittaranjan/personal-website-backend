namespace my_api_app.Core.Filters.Authorization
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SkipApiKeyAuthAttribute : Attribute
    {
    }
}
