using ChoiceVServer.Base;
using Renci.SshNet;
using Renci.SshNet.Security.Cryptography;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ChoiceVServer.Controller {
    public class SecurityController : ChoiceVScript {
        public static string CurrentShopPassword;
        private static RSACryptoServiceProvider RSAProvider;

        public SecurityController() {
            CurrentShopPassword = getRandomPassword(50);
            RSAProvider = new RSACryptoServiceProvider(2048);

            if(Config.IsDiscordBotEnabled) {
                using(var sftpClient = new SftpClient(Config.FtpAddress, 22, Config.FtpUser, Config.FtpPassword)) {
                    sftpClient.Connect();
                    if(sftpClient.IsConnected) {
                        sftpClient.UploadFile(GenerateStreamFromString(getPublicKey()), Config.PublicKeyFileName + ".txt");
                    } else {
                        throw new Exception("Couldnt upload public key");
                    }
                }
            }
        }

        private static Stream GenerateStreamFromString(string s) {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string getPublicKey() {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("-----BEGIN RSA PUBLIC KEY-----");
            builder.AppendLine(Convert.ToBase64String(RSAProvider.ExportRSAPublicKey()));
            builder.AppendLine("-----END RSA PUBLIC KEY-----");

            return builder.ToString();
        }

        public static string decryptMessage(string cypherText) {
            try {
                var bytesCypherText = Convert.FromBase64String(cypherText);
                var bytesPlaintext = RSAProvider.Decrypt(bytesCypherText, false);

                var plainTextData = Encoding.ASCII.GetString(bytesPlaintext);

                return plainTextData;
            } catch(Exception e) {
                Logger.logException(e, "decryptMessage");
                return null;
            }
        }


        public static string EncryptForCef(string source, string key) {
            TripleDESCryptoServiceProvider desCryptoProvider = new TripleDESCryptoServiceProvider();

            byte[] byteBuff;

            try {
                desCryptoProvider.Key = Encoding.UTF8.GetBytes(key);
                desCryptoProvider.IV = UTF8Encoding.UTF8.GetBytes("ILMZRFXQ");
                byteBuff = Encoding.UTF8.GetBytes(source);

                string iv = Convert.ToBase64String(desCryptoProvider.IV);
                Console.WriteLine("iv: {0}", iv);

                string encoded = Convert.ToBase64String(desCryptoProvider.CreateEncryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));

                return encoded;
            } catch(Exception except) {
                Console.WriteLine(except + "\n\n" + except.StackTrace);
                return null;
            }
        }

        public static string DecryptForCef(string encodedText, string key) {
            TripleDESCryptoServiceProvider desCryptoProvider = new TripleDESCryptoServiceProvider();

            byte[] byteBuff;

            try {
                desCryptoProvider.Key = Encoding.UTF8.GetBytes(key);
                desCryptoProvider.IV = UTF8Encoding.UTF8.GetBytes("ILMZRFXQ");
                byteBuff = Convert.FromBase64String(encodedText);

                string plaintext = Encoding.UTF8.GetString(desCryptoProvider.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
                return plaintext;
            } catch(Exception except) {
                Console.WriteLine(except + "\n\n" + except.StackTrace);
                return null;
            }
        }

        //AES STUFF
        //private static readonly int iterations = 1000;

        public static string Encrypt(string input, string password) {
            byte[] encrypted;
            byte[] IV;
            byte[] Salt = GetSalt();
            byte[] Key = CreateKey(password, Salt);

            using(Aes aesAlg = Aes.Create()) {
                aesAlg.Key = Key;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;

                aesAlg.GenerateIV();
                IV = aesAlg.IV;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using(var msEncrypt = new MemoryStream()) {
                    using(var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using(var swEncrypt = new StreamWriter(csEncrypt)) {
                            swEncrypt.Write(input);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            byte[] combinedIvSaltCt = new byte[Salt.Length + IV.Length + encrypted.Length];
            Array.Copy(Salt, 0, combinedIvSaltCt, 0, Salt.Length);
            Array.Copy(IV, 0, combinedIvSaltCt, Salt.Length, IV.Length);
            Array.Copy(encrypted, 0, combinedIvSaltCt, Salt.Length + IV.Length, encrypted.Length);

            return Convert.ToBase64String(combinedIvSaltCt.ToArray());
        }

        public static string Decrypt(string input, string password) {
            byte[] inputAsByteArray;
            string plaintext = null;
            try {
                inputAsByteArray = Convert.FromBase64String(input);

                byte[] Salt = new byte[32];
                byte[] IV = new byte[16];
                byte[] Encoded = new byte[inputAsByteArray.Length - Salt.Length - IV.Length];

                Array.Copy(inputAsByteArray, 0, Salt, 0, Salt.Length);
                Array.Copy(inputAsByteArray, Salt.Length, IV, 0, IV.Length);
                Array.Copy(inputAsByteArray, Salt.Length + IV.Length, Encoded, 0, Encoded.Length);

                byte[] Key = CreateKey(password, Salt);

                using(Aes aesAlg = Aes.Create()) {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using(var msDecrypt = new MemoryStream(Encoded)) {
                        using(var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                            using(var srDecrypt = new StreamReader(csDecrypt)) {
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }

                return plaintext;
            } catch(Exception e) {
                Logger.logException(e, "Decrypt");
                return null;
            }
        }

        public static byte[] CreateKey(string password, byte[] salt) {
            using(var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 1000))
                return rfc2898DeriveBytes.GetBytes(32);
        }

        private static byte[] GetSalt() {
            var salt = new byte[32];
            using(var random = new RNGCryptoServiceProvider()) {
                random.GetNonZeroBytes(salt);
            }

            return salt;
        }

        public static string getRandomPassword(int size) {
            char[] chars =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[size];
            using(RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider()) {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach(byte b in data) {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public static int[] getRandomOrder(int size) {
            var result = new int[size];

            for(int i = 0; i < size; i++) {
                result[i] = i;
            }

            var r = new Random();

            for(int i = 0; i < size; i++) {
                var save = result[i];
                var rand = r.Next(0, size);
                result[i] = result[rand];
                result[rand] = save;
            }

            return result;
        }

        public static string getStringInOrder(string str, int[] order) {
            char[] array = new char[str.Length];
            for(int i = 0; i < str.Length; i++) {
                array[i] = str[order[i]];
            }

            return new string(array);
        }
    }
}
