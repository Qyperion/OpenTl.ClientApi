using System;
using System.IO;
using System.Security.Cryptography;
using OpenTl.Schema;
using OpenTl.Schema.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace OpenTl.Common.Crypto
{
    using DotNetty.Common.Utilities;

    using Extensions;

    public static class RSAHelper
    {
        public static byte[] RsaEncryptWithPublic(byte[] bytesToEncrypt, string publicKey)
        {
            var encryptEngine = new RsaEngine();

            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (RsaKeyParameters) new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyParameter);
            }

            return encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
        }

        public static byte[] RsaEncryptWithPrivate(byte[] bytesToEncrypt, string privateKey)
        {
            var encryptEngine = new RsaEngine();

            using (var txtreader = new StringReader(privateKey))
            {
                var keyParameter = (AsymmetricCipherKeyPair) new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyParameter.Private);
            }

            return encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
        }


        // Decryption:

        public static byte[] RsaDecryptWithPrivate(byte[] bytesToDecrypt, string privateKey)
        {
            var decryptEngine = new RsaEngine();

            using (var txtreader = new StringReader(privateKey))
            {
                var keyParameter = (AsymmetricCipherKeyPair) new PemReader(txtreader).ReadObject();

                decryptEngine.Init(false, keyParameter.Private);
            }

            return decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length);
        }

        public static byte[] RsaDecryptWithPublic(byte[] bytesToDecrypt, string publicKey)
        {
            var decryptEngine = new RsaEngine();

            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (RsaKeyParameters) new PemReader(txtreader).ReadObject();

                decryptEngine.Init(false, keyParameter);
            }

            return decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length);
        }

        public static long GetFingerprint(string key)
        {
            TRsaPublicKey rsaPublicKey;
            using (var txtreader = new StringReader(key))
            {
                var keyParameter = (RsaKeyParameters) new PemReader(txtreader).ReadObject();

                rsaPublicKey = new TRsaPublicKey
                {
                    E = keyParameter.Exponent.ToByteArrayUnsigned(),
                    N = keyParameter.Modulus.ToByteArrayUnsigned()
                };
            }

            var rsaPublicKeyBuffer = Serializer.Serialize(rsaPublicKey);
            byte[] data;
            try
            {
                data = rsaPublicKeyBuffer.ToArray();
            }
            finally
            {
                rsaPublicKeyBuffer.SafeRelease();
            }

            byte[] hash;
            using (var sha1 = SHA1.Create())
            {
                hash = sha1.ComputeHash(data);
            }

            return BitConverter.ToInt64(hash, hash.Length - 8);
        }
    }
}