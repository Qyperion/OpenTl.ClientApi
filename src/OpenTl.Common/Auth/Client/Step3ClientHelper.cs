namespace OpenTl.Common.Auth.Client
{
    using System.Linq;

    using BarsGroup.CodeGuard;

    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    using OpenTl.Common.Crypto;
    using OpenTl.Common.Extensions;
    using OpenTl.Common.GuardExtensions;
    using OpenTl.Schema;
    using OpenTl.Schema.Serialization;

    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Security;

    public static class Step3ClientHelper
    {
        public static RequestSetClientDHParams GetRequest(TServerDHParamsOk serverDhParams, byte[] newNonce, out byte[] clientAgree, out int serverTime)
        {
            AesHelper.ComputeAesParameters(newNonce, serverDhParams.ServerNonce, out var aesKeyData);
            
            var dhInnerData = DeserializeResponse(serverDhParams, aesKeyData);
            serverTime = dhInnerData.ServerTime;
            
            var p = new BigInteger(1, dhInnerData.DhPrimeAsBinary);  
            var g = BigInteger.ValueOf(dhInnerData.G);

            var dhParameters = new DHParameters(p, g);
            KeyGenerationParameters kgp = new DHKeyGenerationParameters(new SecureRandom(), dhParameters);
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            keyGen.Init(kgp);

            var clientKeyPair = keyGen.GenerateKeyPair();
            var publicKey = ((DHPublicKeyParameters)clientKeyPair.Public);

            var y = new BigInteger(1, dhInnerData.GAAsBinary);
            Guard.That(y).IsValidDhPublicKey(dhParameters.P);
            
            var serverPublicKey = new DHPublicKeyParameters(y, dhParameters);
            var clientKeyAgree = AgreementUtilities.GetBasicAgreement("DH");
            clientKeyAgree.Init(clientKeyPair.Private);
            clientAgree = clientKeyAgree.CalculateAgreement(serverPublicKey).ToByteArrayUnsigned();
            
            var clientDhInnerData = new TClientDHInnerData
            {
                RetryId = 0,
                Nonce = serverDhParams.Nonce,
                ServerNonce = serverDhParams.ServerNonce,
                GBAsBinary = publicKey.Y.ToByteArray()
            };
            
            return SerializeRequest(clientDhInnerData, aesKeyData);
        }

        private static RequestSetClientDHParams SerializeRequest(TClientDHInnerData clientDhInnerData, AesKeyData aesKeyData)
        {
            var dhInnerDataBuffer = Serializer.Serialize(clientDhInnerData);

            byte[] innerData;
            try
            {
                Serializer.Serialize(clientDhInnerData);
                innerData = dhInnerDataBuffer.ToArray();
            }
            finally
            {
                dhInnerDataBuffer.SafeRelease();
            }
            
            var hashsum = Sha1Helper.ComputeHashsum(innerData);

            var answerWithHash = hashsum.Concat(innerData).ToArray();

            var encryptedAnswer = AES.EncryptAes(aesKeyData, answerWithHash);

            return new RequestSetClientDHParams
                   {
                       EncryptedDataAsBinary = encryptedAnswer,
                       Nonce = clientDhInnerData.Nonce,
                       ServerNonce = clientDhInnerData.ServerNonce
                   };
        }

        private static TServerDHInnerData DeserializeResponse(TServerDHParamsOk serverDhParams, AesKeyData aesKeyData)
        {
            var answerWithHash = AES.DecryptAes(aesKeyData, serverDhParams.EncryptedAnswerAsBinary);

            var answerWithHashBuffer = PooledByteBufferAllocator.Default.Buffer();

            try
            {
                answerWithHashBuffer.WriteBytes(answerWithHash);

                // var serverHashsum = answerWithHashBuffer.ToArray(20);
                answerWithHashBuffer.SkipBytes(20);

                var serverDhInnerData = (TServerDHInnerData)Serializer.Deserialize(answerWithHashBuffer);

                // var clearAnswer = Serializer.Serialize(serverDhInnerData);
                // var hashsum = SHA1Helper.ComputeHashsum(clearAnswer);
                // Guard.That(serverHashsum).IsItemsEquals(hashsum);

                return serverDhInnerData;
            }
            finally
            {
                answerWithHashBuffer.SafeRelease();
            }
        }
    }
}