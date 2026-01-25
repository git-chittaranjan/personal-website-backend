using System.Security.Cryptography;

namespace my_api_app.Helpers
{
    public class OtpGenerator
    {
        public static string GenerateSixDigitOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var code = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return code.ToString("D6");
        }
    }
}
