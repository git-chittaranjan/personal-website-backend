using my_api_app.Models;
using System.Threading.Tasks;

namespace my_api_app.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAndStoreOtpAsync(Guid userID, string email, OtpPurpose purpose);
        Task<bool> ValidateOtpAsync(Guid userId, string email, string otp, OtpPurpose purpose);
    }
}
