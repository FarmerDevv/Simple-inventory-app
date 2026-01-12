using System;
using System.Security.Cryptography;

namespace PlusBin.Utils
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 100_000;

        public static (byte[] hash, byte[] salt) HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Şifre boş olamaz.", nameof(password));

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );
            return (hash, salt);
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrEmpty(password) || storedHash == null || storedSalt == null)
                return false;

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                storedSalt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );

            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}