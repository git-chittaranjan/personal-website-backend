using System;

namespace my_api_app.Services
{
    public interface IPasswordHasher
    {
        (byte[] hash, byte[] salt) HashPassword(string password);
        bool VerifyPassword(string password, byte[] salt, byte[] hash);
    }
}
