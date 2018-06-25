namespace PlaylistBaseLibrary.Common.Security
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    public class Cryptography
    {

        public static X509Certificate2 GetCertificate(string certificateName, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            X509Store my = new X509Store(storeName, storeLocation);
            my.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection collection = my.Certificates.Find(X509FindType.FindBySubjectName, certificateName, false);
            if (collection.Count == 1)
            {
                return collection[0];
            }
            else if (collection.Count > 1)
            {
                throw new Exception(string.Format("More than one certificate with name '{0}' found in store LocalMachine/My.", certificateName));
            }
            else
            {
                throw new Exception(string.Format("Certificate '{0}' not found in store LocalMachine/My.", certificateName));
            }
        }

        public static byte[] EncryptRsa(byte[] input, string certificateName)
        {
            var output = string.Empty;
            X509Certificate2 cert = GetCertificate(certificateName);
            using (var csp = cert.GetRSAPublicKey())
            {
                byte[] bytesEncrypted = csp.Encrypt(input, RSAEncryptionPadding.OaepSHA1);
                output = Convert.ToBase64String(bytesEncrypted);
            }
            return Encoding.UTF8.GetBytes(output);
        }

        public static byte[] DecryptRsa(byte[] encrypted, string certificateName)
        {
            var text = Encoding.UTF8.GetString(encrypted);

            X509Certificate2 cert = GetCertificate(certificateName);
            if (cert.HasPrivateKey)
                using (var csp = cert.GetRSAPrivateKey())
                {
                    byte[] bytesEncrypted = Convert.FromBase64String(text);
                    return csp.Decrypt(bytesEncrypted, RSAEncryptionPadding.OaepSHA1);
                }
            return null;
        }

        public static string EncryptRsa(string input, string certificateName)
        {
            var output = string.Empty;

            X509Certificate2 cert = GetCertificate(certificateName);
            if (cert.HasPrivateKey)
                using (var csp = cert.GetRSAPublicKey())
                {
                    byte[] bytesData = Encoding.UTF8.GetBytes(input);
                    byte[] bytesEncrypted = csp.Encrypt(bytesData, RSAEncryptionPadding.OaepSHA1);
                    output = Convert.ToBase64String(bytesEncrypted);
                }
            return output;
        }

        public static string DecryptRsa(string encrypted, string certificateName)
        {
            var text = string.Empty;
            X509Certificate2 cert = GetCertificate(certificateName);
            using (var csp = (RSACryptoServiceProvider)cert.PrivateKey)
            {
                byte[] bytesEncrypted = Convert.FromBase64String(encrypted);
                byte[] bytesDecrypted = csp.Decrypt(bytesEncrypted, false);
                text = Encoding.UTF8.GetString(bytesDecrypted);
            }
            return text;
        }

        /// <summary>
        /// Encrypt with Symetric algo and encryp the with asym and push it to the begin of string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="keyString"></param>
        /// <param name="certificateName"></param>
        /// <returns></returns>
        public static string EncryptString(string text, string keyString, string certificateName = "")
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;
                        // Encrypt Key with certificate (iv)
                        if (!string.IsNullOrEmpty(certificateName))
                            iv = EncryptRsa(iv, certificateName);

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public static string DecryptString(string cipherText, string keyString, string certificateName = "")
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            //TODO: must be calculated dynamically
            var encodedVILength = 344;
            var iv = new byte[encodedVILength];
            var cipher = new byte[fullCipher.Length - encodedVILength];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - encodedVILength);
            var key = Encoding.UTF8.GetBytes(keyString);
            // Decrypt Key with certificate
            if (!string.IsNullOrEmpty(certificateName))
                key = DecryptRsa(iv, certificateName);
            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(Encoding.UTF8.GetBytes(keyString), key))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public static int GetMaxDataLength(int keySize, bool optimalAsymmetricEncryptionPadding)
        {
            if (optimalAsymmetricEncryptionPadding)
            {
                return ((keySize - 384) / 8) + 7;
            }
            return ((keySize - 384) / 8) + 37;
        }

        public static bool IsKeySizeValid(int keySize)
        {
            return keySize >= 384 &&
                    keySize <= 16384 &&
                    keySize % 8 == 0;
        }
    }
}
