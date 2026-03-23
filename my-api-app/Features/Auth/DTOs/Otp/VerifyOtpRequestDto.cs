using my_api_app.Domain.Enums;

namespace my_api_app.Features.Auth.DTOs.Otp
{
    public class VerifyOtpRequestDto
    {
        public string Email { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
        public OtpPurpose OtpPurpose { get; set; } = default!;
    }
}
