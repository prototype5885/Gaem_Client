using System.Text;
using System.Security.Cryptography;
using System;

public class PasswordHasher
{
    public string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // Compute the hash of the password
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Convert the byte array to a hexadecimal string
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}
