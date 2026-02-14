using Microsoft.AspNetCore.Mvc;
using my_api_app.DTOs.Auth;
using my_api_app.DTOs.Auth.Login;
using my_api_app.DTOs.Auth.Password_Reset;
using my_api_app.DTOs.Auth.Register;
using my_api_app.Services.Auth;

namespace my_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(IAuthService authService, IPasswordResetService passwordResetService)
        {
            _authService = authService;
            _passwordResetService = passwordResetService;
        }



        // ------------------------------
        // Register
        // ------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequestDto dto, CancellationToken cancellationToken)
        {
            await _authService.RegisterUserAsync(dto, cancellationToken);
            return Ok(new { message = "OTP sent to email." });
        }



        // ------------------------------
        // Login
        // ------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequestDto dto, CancellationToken cancellationToken)
        {
            await _authService.LoginUserAsync(dto, cancellationToken);
            return Ok(new { message = "OTP sent to registered email." });
        }



        // ------------------------------
        // Verify OTP
        // ------------------------------
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequestDto dto, CancellationToken cancellationToken)
        {
            var result = await _authService.VerifyOtpAsync(dto, cancellationToken);
            return Ok(result);
        }



        // ------------------------------
        // Forgot Password : Validate Email and Send OTP
        // ------------------------------
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto, CancellationToken cancellationToken)
        {
            await _passwordResetService.ForgotPasswordAsync(dto.Email, cancellationToken);
            return Ok(new { message = "OTP sent if email exists." });
        }



        // ------------------------------
        // Reset Password
        // ------------------------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto, CancellationToken cancellationToken)
        {
            await _passwordResetService.ResetPasswordAsync(dto.Email, dto.ResetToken, dto.NewPassword, cancellationToken);

            return Ok(new ResetPasswordResponse
            {
                Message = "Password reset successful."
            });
        }
    }
}
