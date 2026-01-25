using my_api_app.Enums;

namespace my_api_app.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        public string Email { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
        public OtpPurpose OtpPurpose { get; set; } = default!;
    }
}
