using Microsoft.AspNetCore.Http.HttpResults;
using my_api_app.DTOs.Auth;
using my_api_app.DTOs.Auth.Login;
using my_api_app.DTOs.Auth.Register;
using my_api_app.Enums;
using my_api_app.Exceptions.BusinessExceptions.OtpExceptions;
using my_api_app.Exceptions.BusinessExceptions.ServerExceptions;
using my_api_app.Exceptions.BusinessExceptions.UserExceptions;
using my_api_app.Models.Auth;
using my_api_app.Repositories.Interfaces;
using my_api_app.Services.Security.Interfaces;

namespace my_api_app.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPendingUserRepository _pendingUserRepo;
        private readonly IPasswordHasher _hasher;
        private readonly IOtpService _otpService;
        private readonly ITokenService _tokenService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthService(IUserRepository userRepo, IPendingUserRepository pendingUserRepository, IPasswordHasher hasher, IOtpService otpService, ITokenService tokenService, IPasswordResetService passwordResetService)
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

            var pendingUser = new User
            {
                Name = dto.Name,
                Gender = dto.Gender,
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsEmailVerified = false
            };

            await _pendingUserRepo.CreatePendingUserAsync(pendingUser, cancellationToken);

            await _otpService.GenerateAndSendOtpAsync(dto.Name, dto.Email, OtpPurpose.EMAIL_VERIFICATION, cancellationToken);
        }



        // ------------------------------
        // Login validation and send OTP
        // ------------------------------
        public async Task LoginUserAsync(UserLoginRequestDto dto, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetUserAsync(dto.Email, cancellationToken);

            if (user == null)
                throw new InvalidCredentialsException();

            var passwordFlag = _hasher.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt);

            if (!passwordFlag)
                throw new InvalidCredentialsException();

            await _otpService.GenerateAndSendOtpAsync(user.Name, dto.Email, OtpPurpose.LOGIN, cancellationToken);
        }




        // ------------------------------
        // Validate OTP and initiate Registration/Login/Reset process
        // ------------------------------
        public async Task<object> VerifyOtpAsync(VerifyOtpRequestDto dto, CancellationToken cancellationToken)
        {
            await _otpService.ValidateOtpAsync(dto.Email, dto.OtpCode, dto.OtpPurpose, cancellationToken);

            return dto.OtpPurpose switch
            {
                OtpPurpose.EMAIL_VERIFICATION =>
                    await CompleteRegistrationAsync(dto.Email, cancellationToken),

                OtpPurpose.LOGIN =>
                    await CompleteLoginAsync(dto.Email, cancellationToken),

                OtpPurpose.PASSWORD_RESET =>
                    await _passwordResetService.GenerateResetTokenAsync(dto.Email, cancellationToken),

                _ => throw new UnsupportedOtpPurposeException()
            };
        }



        // ------------------------------
        // Register: Create User in Users Table
        // ------------------------------
        private async Task<object> CompleteRegistrationAsync(string email, CancellationToken cancellationToken)
        {
            var pendingUser = await _pendingUserRepo.GetPendingUserAsync(email, cancellationToken);

            if (pendingUser == null)
                throw new PendingUserNotFoundException();

            await _userRepo.CreateUserAsync(pendingUser.Name, pendingUser.Email, pendingUser.Gender, pendingUser.PasswordHash, pendingUser.PasswordSalt, cancellationToken);

            await _pendingUserRepo.DeletePendingUserAsync(email, cancellationToken);

            return new
            {
                message = "User registered successfully."
            };
        }




        // ------------------------------
        // Login: Generate JWT Token
        // ------------------------------
        private async Task<object> CompleteLoginAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetUserAsync(email, cancellationToken);

            if (user is null)
                throw new InternalServerException();

            var loginResponse = _tokenService.GenerateAccessToken(user, JwtTokenPurpose.LOGIN);

            if (loginResponse is null)
                throw new InternalServerException();

            return loginResponse;
        }        
    }
}
