using my_api_app.Enums;

namespace my_api_app.Models.Auth
{
    public sealed class OtpFlowResult
    {
        public OtpPurpose OtpFlowPurpose { get; init; }
        public object? Data { get; init; }
    }
}
