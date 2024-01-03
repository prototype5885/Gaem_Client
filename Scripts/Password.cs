using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace ProToTypeLounge.Password
{
    public class Password
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Compute the hash of the password
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert the byte array to a hexadecimal string
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
        public static bool ValidateCredentials(string enteredPassword, string storedHashedPassword)
        {
            string enteredHashedPassword = HashPassword(enteredPassword);
            //return enteredHashedPassword == storedHashedPassword;
            string hashedpassword = "1837bc2c546d46c705204cf9f857b90b1dbffd2a7988451670119945ba39a10b";
            return enteredHashedPassword == hashedpassword;
        }
        //static byte[] EncryptAES256(string plainText, string key, string iv)
        //{
        //    using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        //    {
        //        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        //        aesAlg.IV = Encoding.UTF8.GetBytes(iv);

        //        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        //        using (MemoryStream msEncrypt = new MemoryStream())
        //        {
        //            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        //                {
        //                    swEncrypt.Write(plainText);
        //                }
        //            }

        //            return msEncrypt.ToArray();
        //        }
        //    }
        //}

    }
}
