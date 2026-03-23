using my_api_app.Domain.Models;
using my_api_app.Features.Auth.DTOs.Login;
using my_api_app.Features.Auth.DTOs.Otp;
using my_api_app.Features.Auth.DTOs.Register;
using System.Security.Claims;
using System.Threading;

namespace my_api_app.Features.Auth.Services
{
    public interface IAuthService
    {
        Task RegisterUserAsync(UserRegisterRequestDto dto, CancellationToken cancellationToken);
        Task LoginUserAsync(UserLoginRequestDto dto, CancellationToken cancellationToken);
        Task<OtpFlowResult> VerifyOtpAsync(VerifyOtpRequestDto dto, CancellationToken cancellationToken);        
    }
}
