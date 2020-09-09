using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace S031.MetaStack.Security
{
    public static class CryptoHelper
    {
        /// <summary>
        /// Compute hash by HashAlgorithm
        /// </summary>
        /// <param name="hashAlgorithm"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ComputeHash(HashAlgorithm hashAlgorithm, string input)
            => BitConverter
            .ToString(
                hashAlgorithm
                .ComputeHash(Encoding.UTF8.GetBytes(input)));
        /// <summary>
        /// Compute SHA256 hash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ComputeSha256Hash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return ComputeHash(sha256Hash, input)
                    .RemoveChar('-');
            }
        }
        public static string ComputeMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                return ComputeHash(md5, input)
                    .RemoveChar('-');
            }
        }
    }
}
