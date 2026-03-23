using System.Security.Cryptography;
using System.Text;

namespace my_api_app.Infrastructure.Security.Hasher
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
