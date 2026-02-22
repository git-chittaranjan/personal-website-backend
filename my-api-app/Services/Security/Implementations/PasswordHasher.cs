using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using my_api_app.Services.Security.Interfaces;

namespace my_api_app.Services.Security.Implementations
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128-bit
        private const int Iterations = 120_000; // high iteration count
        private const int KeySize = 32; // 256-bit key

        public (byte[] hash, byte[] salt) HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

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
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            if (storedHash == null || storedHash.Length == 0)
                throw new ArgumentNullException(nameof(storedHash));
            if (storedSalt == null || storedSalt.Length == 0)
                throw new ArgumentNullException(nameof(storedSalt));

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
