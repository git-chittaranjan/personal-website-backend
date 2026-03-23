using my_api_app.Core.Exceptions.BusinessExceptions.OtpExceptions;
using my_api_app.Core.Exceptions.BusinessExceptions.ServerExceptions;
using my_api_app.Core.Exceptions.BusinessExceptions.TokenExceptions;
using my_api_app.Core.Exceptions.BusinessExceptions.UserExceptions;
using my_api_app.Domain.Enums;
using my_api_app.Domain.Models;
using my_api_app.Features.Auth.DTOs.Login;
using my_api_app.Features.Auth.DTOs.Otp;
using my_api_app.Features.Auth.DTOs.Register;
using my_api_app.Infrastructure.OTP;
using my_api_app.Infrastructure.Security.Hasher;
using my_api_app.Infrastructure.Security.Token;
using my_api_app.Repositories.User;
using my_api_app.Repositories.UserRepo;

namespace my_api_app.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPendingUserRepository _pendingUserRepo;
        private readonly IPasswordHasher _hasher;
        private readonly IOtpService _otpService;
        private readonly IJwtTokenService _tokenService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthService(IUserRepository userRepo, IPendingUserRepository pendingUserRepository, IPasswordHasher hasher, IOtpService otpService, IJwtTokenService tokenService, IPasswordResetService passwordResetService)
        {
            _userRepo = userRepo;
            _pendingUserRepo = pendingUserRepository;
            _hasher = hasher;
            _otpService = otpService;
            _tokenService = tokenService;
            _passwordResetService = passwordResetService;
        }



        // ------------------------------
        // Send OTP and Create User in PendingUsers Table
        // ------------------------------
        public async Task RegisterUserAsync(UserRegisterRequestDto dto, CancellationToken cancellationToken)
        {
            bool emailExists = await _userRepo.EmailExistsAsync(dto.Email, cancellationToken);

            if (emailExists)
                throw new UserAlreadyExistsException();


            var (hash, salt) = _hasher.HashPassword(dto.Password);

            var pendingUser = new Domain.Models.User
            {
                Name = dto.Name,
                Gender = dto.Gender,
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsEmailVerified = false
            };

            await _pendingUserRepo.CreatePendingUserAsync(pendingUser, cancellationToken);

            try
            {
                await _otpService.GenerateAndSendOtpAsync(dto.Name, dto.Email, OtpPurpose.EMAIL_VERIFICATION, cancellationToken);
            }
            catch (OtpDeliveryFailedException)
            {
                await _pendingUserRepo.DeletePendingUserAsync(dto.Email, cancellationToken);
                throw; // Because email delivery is an external call that can fail independently and deserves its own meaningful error code rather than a generic 500
            }
        }



        // ------------------------------
        // Login validation and send OTP
        // ------------------------------
        public async Task LoginUserAsync(UserLoginRequestDto dto, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetUserByEmailAsync(dto.Email, cancellationToken);

            if (user == null)
                throw new InvalidCredentialsException();

            var passwordFlag = _hasher.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt);

            if (!passwordFlag)
                throw new InvalidCredentialsException();

            try
            {
                await _otpService.GenerateAndSendOtpAsync(user.Name, dto.Email, OtpPurpose.LOGIN, cancellationToken);
            }
            catch (OtpDeliveryFailedException)
            {
                throw;
            }
        }




        // ------------------------------
        // Validate OTP and initiate Registration/Login/Reset process
        // ------------------------------
        public async Task<OtpFlowResult> VerifyOtpAsync(VerifyOtpRequestDto dto, CancellationToken cancellationToken)
        {
            Guid otpId = await _otpService.ValidateOtpAsync(dto.Email, dto.OtpCode, dto.OtpPurpose, cancellationToken);

            if(otpId == Guid.Empty)
                throw new InvalidOtpException();

            return dto.OtpPurpose switch
            {
                OtpPurpose.EMAIL_VERIFICATION => new OtpFlowResult
                {
                    OtpFlowPurpose = OtpPurpose.EMAIL_VERIFICATION,
                    Data = await CompleteRegistrationAsync(dto.Email, cancellationToken)
                },

                OtpPurpose.LOGIN => new OtpFlowResult
                {
                    OtpFlowPurpose = OtpPurpose.LOGIN,
                    Data = await CompleteLoginAsync(dto.Email, cancellationToken)
                },

                OtpPurpose.PASSWORD_RESET => new OtpFlowResult
                {
                    OtpFlowPurpose = OtpPurpose.PASSWORD_RESET,
                    Data = await _passwordResetService.GenerateResetTokenAsync(dto.Email, cancellationToken)
                },

                _ => throw new UnsupportedOtpPurposeException()
            };
        }



        // ------------------------------
        // Register: Create User in Users Table
        // ------------------------------
        private async Task<object> CompleteRegistrationAsync(string email, CancellationToken cancellationToken)
        {
            bool emailExists = await _userRepo.EmailExistsAsync(email, cancellationToken);

            // Handling Race Condition resulted in the email already existing
            if (emailExists)
                throw new UserAlreadyExistsException();

            var pendingUser = await _pendingUserRepo.GetPendingUserAsync(email, cancellationToken);

            if (pendingUser == null)
                throw new PendingUserNotFoundException();

            CreatedUserResult result = await _userRepo.CreateUserAsync(pendingUser.Name, pendingUser.Email, pendingUser.Gender, pendingUser.PasswordHash, pendingUser.PasswordSalt, cancellationToken);

            await _pendingUserRepo.DeletePendingUserAsync(email, cancellationToken);

            return new UserRegisterResponseDto
            {
                UserId = result.UserID,
                Email = email,
                CreatedAt = result.CreatedAt
            };
        }




        // ------------------------------
        // Login: Generate JWT Token
        // ------------------------------
        private async Task<object> CompleteLoginAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetUserByEmailAsync(email, cancellationToken);

            if (user is null)
                throw new InternalServerException();

            UserLoginResponseDto? loginResponse = _tokenService.GenerateAccessToken(user, JwtTokenPurpose.LOGIN);

            if (loginResponse is null)
                throw new TokenGenerationFailedException();

            return new UserLoginResponseDto
            {
                UserId = loginResponse.UserId,
                Email = loginResponse.Email,
                Name = loginResponse.Name,
                AccessToken = loginResponse.AccessToken,
                ExpiresAt = loginResponse.ExpiresAt,
                TokenType = loginResponse.TokenType
            };
        }
    }
}
