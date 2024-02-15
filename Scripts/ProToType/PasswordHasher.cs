﻿using System.Text;
using System.Security.Cryptography;
using System;

public class PasswordHasher : IDisposable
{
    private bool disposed = false;

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
    // Implement IDisposable
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Clean up managed resources
            }


            disposed = true;
        }
    }
}
