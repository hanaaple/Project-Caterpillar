using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utility.JsonLoader
{
    public static class JsonDataDecryptor
    {
        private static readonly string SecurityPassword = "A1VtcX729XV6D4aXIM1PMB1";

        public static string AesDecrypt256(string inputText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            byte[] encryptedData = Convert.FromBase64String(inputText);
            byte[] salt = Encoding.ASCII.GetBytes(SecurityPassword.Length.ToString());

            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(SecurityPassword, salt);

            ICryptoTransform decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));

            MemoryStream memoryStream = new MemoryStream(encryptedData);

            // 데이터 읽기 용도의 cryptoStream객체
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            // 복호화된 데이터를 담을 바이트 배열을 선언한다.
            byte[] plainText = new byte[encryptedData.Length];

            int decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);

            memoryStream.Close();
            cryptoStream.Close();

            string decryptedData = Encoding.Unicode.GetString(plainText, 0, decryptedCount);

            return decryptedData;
        }
    }
}
