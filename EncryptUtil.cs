using Base62;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PartnersPlatform.Utility
{
    public class EncryptUtil
    {
        public EncryptUtil()
        {
        }
        private string GetKeyToday()
        {
            return "/B?E(H+MbQeThWmZ";
        }
        public async Task<string> aesDecryptAsync(string content)
        {
            return await RecursiveDecryptAES(content);
        }
        private async Task<string> RecursiveDecryptAES(string content)
        {
            if (content != null)
                return await InternalDecryptAESAsync(content);
            else
                return null;
        }
        private async Task<string> InternalDecryptAESAsync(string content)
        {
            string KeyToday = GetKeyToday();
            byte[] key = Encoding.UTF8.GetBytes(GetKeyToday());
            byte[] iv = new byte[key.Length];

            string plaintext = null;
            byte[] cipherText = content.FromBase62();

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = await srDecrypt.ReadToEndAsync();
                        }
                    }
                }
            }
            return plaintext;
        }

        //Encryption
        public async Task<string> aesEncryptAsync(string content)
        {
            return await RecursiveEncryptAES(content);
        }
        private async Task<string> RecursiveEncryptAES(string content)
        {
            if (content != null)
            {
                return await InternalEncryptAESAsync(content);
            }
            else
            {
                return null;
            }
        }
        private async Task<string> InternalEncryptAESAsync(string content)
        {
            string KeyToday = GetKeyToday();
            byte[] key = Encoding.UTF8.GetBytes(GetKeyToday());
            byte[] iv = new byte[key.Length];
            byte[] encrypted;


            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            await swEncrypt.WriteAsync(content);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            string convertedstring = encrypted.ToBase62();
            return convertedstring;
        }
    }
}
