using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_api_app.Core.Exceptions.BusinessExceptions.OtpExceptions;
using my_api_app.Core.Responses;
using my_api_app.Domain.Enums;
using my_api_app.Domain.Models;
using my_api_app.DTOs.Auth;
using my_api_app.Features.Auth.DTOs.Login;
using my_api_app.Features.Auth.DTOs.Otp;
using my_api_app.Features.Auth.DTOs.Register;
using my_api_app.Features.Auth.Services;

namespace my_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IPasswordResetService passwordResetService, IApiResponseFactory responseFactory, ILogger<AuthController> logger)
            : base(responseFactory)
        {
            _authService = authService;
            _passwordResetService = passwordResetService;
            _logger = logger;
        }



        // ------------------------------
        // Register
        // ------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequestDto dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Registration attempt for email {Email}", dto.Email);

            await _authService.RegisterUserAsync(dto, cancellationToken);

            _logger.LogInformation("Registration verification OTP sent successful to {Email}", dto.Email);

            return SuccessResponse(Statuses.OtpSent);
        }



        // ------------------------------
        // Login
        // ------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Login attempt for email {Email}", dto.Email);

            await _authService.LoginUserAsync(dto, cancellationToken);

            _logger.LogInformation("Login verification OTP sent to {Email}", dto.Email);

            return SuccessResponse(Statuses.OtpSent);
        }



        // ------------------------------
        // Verify OTP
        // ------------------------------
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OTP verification attempt for email {Email}, purpose {Purpose}", dto.Email, dto.OtpPurpose);

            OtpFlowResult result = await _authService.VerifyOtpAsync(dto, cancellationToken);

            switch (result.OtpFlowPurpose)
            {
                case OtpPurpose.EMAIL_VERIFICATION:
                    {
                        if (result.Data is not UserRegisterResponseDto registerData) //If resullt.Data of type UserRegisterResponseDto, then assign it to variable registerData.
                        {
                            _logger.LogError("EMAIL_VERIFICATION - OTP flow data mismatch — expected UserRegisterResponseDto for email {Email}", dto.Email);

                            throw new InvalidOperationException("Unexpected OTP flow data type for EMAIL_VERIFICATION.");
                        }

                        var resourceUrl = $"api/user/{registerData.UserId}";

                        _logger.LogInformation("Email verified — user registered successfully, UserId {UserId}", registerData.UserId);

                        return CreatedResponse(Statuses.UserCreated, registerData, resourceUrl);
                    }

                case OtpPurpose.LOGIN:
                    {
                        if (result.Data is not UserLoginResponseDto loginData)
                        {
                            _logger.LogError("LOGIN - OTP flow data mismatch — expected UserLoginResponseDto for email {Email}", dto.Email);
                            throw new InvalidOperationException("Unexpected OTP flow data type for LOGIN.");
                        }

                        _logger.LogInformation("OTP login successful for email {Email}", dto.Email);

                        return SuccessResponse(Statuses.Success, loginData);
                    }

                case OtpPurpose.PASSWORD_RESET:
                    {
                        if (result.Data is not ForgotPasswordResponseDto resetData)
                        {
                            _logger.LogError("PASSWORD_RESET - OTP flow data mismatch — expected ForgotPasswordResponseDto for email {Email}", dto.Email);
                            throw new InvalidOperationException("Unexpected OTP flow data type for PASSWORD_RESET.");
                        }

                        _logger.LogInformation("Password reset OTP verified for email {Email}", dto.Email);

                        return SuccessResponse(Statuses.Success, resetData);
                    }

                default:
                    _logger.LogWarning("Unsupported OTP purpose {Purpose} for email {Email}", result.OtpFlowPurpose, dto.Email);
                    throw new UnsupportedOtpPurposeException();
            }
        }



        // ------------------------------
        // Forgot Password : Validate Email and Send OTP
        // ------------------------------
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Forgot password request for email {Email}", dto.Email);

            await _passwordResetService.ForgotPasswordAsync(dto.Email, cancellationToken);

            _logger.LogInformation("Password reset OTP sent to {Email}", dto.Email);

            return SuccessResponse(Statuses.PasswordResetOtpSent);
        }



        // ------------------------------
        // Reset Password
        // ------------------------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Password reset attempt for email {Email}", dto.Email);

            await _passwordResetService.ResetPasswordAsync(dto.Email, dto.ResetToken, dto.NewPassword, cancellationToken);

            _logger.LogInformation("Password reset successful for email {Email}", dto.Email);

            return SuccessResponse(Statuses.PasswordResetSuccess);
        }
    }
}
