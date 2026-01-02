using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace my_api_app.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128-bit
        private const int Iterations = 120_000; // high iteration count
        private const int KeySize = 32; // 256-bit key

        public (byte[] hash, byte[] salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            var derived = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            return (derived, salt);
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            var derived = KeyDerivation.Pbkdf2(
                password: password,
                salt: storedSalt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            return CryptographicOperations.FixedTimeEquals(derived, storedHash);
        }
    }
}
