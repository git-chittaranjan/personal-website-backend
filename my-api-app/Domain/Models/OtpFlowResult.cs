using my_api_app.Domain.Enums;

namespace my_api_app.Domain.Models
{
    public sealed class OtpFlowResult
    {
        public OtpPurpose OtpFlowPurpose { get; init; }
        public object? Data { get; init; }
    }
}
