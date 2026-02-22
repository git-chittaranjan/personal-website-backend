using my_api_app.DTOs.Auth;
using my_api_app.DTOs.Auth.Login;
using my_api_app.DTOs.Auth.Register;
using my_api_app.Models.Auth;
using System.Security.Claims;
using System.Threading;

namespace my_api_app.Services.Auth
{
    public interface IAuthService
    {
        Task RegisterUserAsync(UserRegisterRequestDto dto, CancellationToken cancellationToken);
        Task LoginUserAsync(UserLoginRequestDto dto, CancellationToken cancellationToken);
        Task<OtpFlowResult> VerifyOtpAsync(VerifyOtpRequestDto dto, CancellationToken cancellationToken);        
    }
}
