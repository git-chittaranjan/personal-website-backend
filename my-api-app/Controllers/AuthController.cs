using Microsoft.AspNetCore.Mvc;
using my_api_app.DTOs.Auth;
using my_api_app.DTOs.Auth.Login;
using my_api_app.DTOs.Auth.Register;
using my_api_app.Enums;
using my_api_app.Exceptions.BusinessExceptions.OtpExceptions;
using my_api_app.Models.Auth;
using my_api_app.Responses;
using my_api_app.Services.Auth;

namespace my_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(IAuthService authService, IPasswordResetService passwordResetService, IApiResponseFactory responseFactory)
            : base(responseFactory)
        {
            _authService = authService;
            _passwordResetService = passwordResetService;
        }



        // ------------------------------
        // Register
        // ------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequestDto dto, CancellationToken cancellationToken)
        {
            await _authService.RegisterUserAsync(dto, cancellationToken);
            return SuccessResponse(Statuses.OtpSent);
        }



        // ------------------------------
        // Login
        // ------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto dto, CancellationToken cancellationToken)
        {
            await _authService.LoginUserAsync(dto, cancellationToken);
            return SuccessResponse(Statuses.OtpSent);
        }



        // ------------------------------
        // Verify OTP
        // ------------------------------
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto, CancellationToken cancellationToken)
        {
            OtpFlowResult result = await _authService.VerifyOtpAsync(dto, cancellationToken);

            switch (result.OtpFlowPurpose)
            {
                case OtpPurpose.EMAIL_VERIFICATION:
                    {
                        UserRegisterResponseDto? data = (UserRegisterResponseDto)result.Data!;
                        var resourceUrl = $"api/user/{data.UserId}";

                        // Logic to send Welcome Email after successful registration

                        return CreatedResponse(Statuses.UserCreated, data, resourceUrl);
                    }

                case OtpPurpose.LOGIN:
                    {
                        UserLoginResponseDto? loginData = (UserLoginResponseDto)result.Data!;

                        return SuccessResponse(Statuses.Success, loginData);
                    }

                case OtpPurpose.PASSWORD_RESET:
                    {
                        ForgotPasswordResponseDto resetData = (ForgotPasswordResponseDto)result.Data!;

                        return SuccessResponse(Statuses.Success, resetData);
                    }

                default:
                    throw new UnsupportedOtpPurposeException();
            }
        }



        // ------------------------------
        // Forgot Password : Validate Email and Send OTP
        // ------------------------------
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken cancellationToken)
        {
            await _passwordResetService.ForgotPasswordAsync(dto.Email, cancellationToken);
            return SuccessResponse(Statuses.PasswordResetOtpSent);
        }



        // ------------------------------
        // Reset Password
        // ------------------------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken cancellationToken)
        {
            await _passwordResetService.ResetPasswordAsync(dto.Email, dto.ResetToken, dto.NewPassword, cancellationToken);

            return SuccessResponse(Statuses.PasswordResetSuccess);
        }
    }
}
