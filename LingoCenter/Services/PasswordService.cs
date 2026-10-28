using System;
using System.Security.Cryptography;

namespace LingoCenter.Services
{
    public static class PasswordService
    {
        public static string Hash(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            using (var kdf = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
                return "100000." + Convert.ToBase64String(salt) + "." + Convert.ToBase64String(kdf.GetBytes(32));
        }

        public static bool Verify(string password, string hash)
        {
            try
            {
                var p = hash.Split('.');
                using (var kdf = new Rfc2898DeriveBytes(password, Convert.FromBase64String(p[1]), int.Parse(p[0]), HashAlgorithmName.SHA256))
                {
                    var a = kdf.GetBytes(32);
                    var b = Convert.FromBase64String(p[2]);
                    int d = a.Length ^ b.Length;
                    for (int i = 0; i < a.Length && i < b.Length; i++)
                        d |= a[i] ^ b[i];
                    return d == 0;
                }
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
