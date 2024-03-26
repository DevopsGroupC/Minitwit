using System;
using System.Security.Cryptography;
using System.Text;

namespace csharp_minitwit.Utils
{
    public static class GravatarHelper
    {
        public static string GetGravatarUrl(string email, int size = 80)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(email.Trim().ToLower());
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("X2"));
                }

                return $"http://www.gravatar.com/avatar/{sb.ToString().ToLower()}?d=identicon&s={size}";
            }
        }
    }
}