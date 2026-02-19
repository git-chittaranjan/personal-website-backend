using my_api_app.Services.Security.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace my_api_app.Services.Security.Implementations
{
    public class ResetTokenHasher : IResetTokenHasher
    {
        public byte[] Hash(string input)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        }
    }
}
