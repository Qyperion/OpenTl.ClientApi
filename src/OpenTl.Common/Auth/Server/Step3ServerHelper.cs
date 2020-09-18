namespace OpenTl.Common.Auth.Server
{
    using System.Linq;

    using BarsGroup.CodeGuard;

    using DotNetty.Buffers;

    using OpenTl.Common.Crypto;
    using OpenTl.Schema;
    using OpenTl.Schema.Serialization;

    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Security;
    
    using OpenTl.Common.GuardExtensions;

    public static class Step3ServerHelper
    {
        public static ISetClientDHParamsAnswer GetResponse(RequestSetClientDHParams setClientDhParams, byte[] newNonce, AsymmetricCipherKeyPair serverKeyPair, out BigInteger serverAgree, out byte[] serverSalt)
        {
            AesHelper.ComputeAesParameters(newNonce, setClientDhParams.ServerNonce, out var aesKeyData);
            
            var dhInnerData = DeserializeRequest(setClientDhParams, aesKeyData);

            var dhParameters = ((DHPrivateKeyParameters)serverKeyPair.Private).Parameters;
            
            var y = new BigInteger(dhInnerData.GBAsBinary);
            Guard.That(y).IsValidDhPublicKey(dhParameters.P);

            var clientPublicKey = new DHPublicKeyParameters(y, dhParameters);
                
            var serverKeyAgree = AgreementUtilities.GetBasicAgreement("DH");
            serverKeyAgree.Init(serverKeyPair.Private);

            serverAgree = serverKeyAgree.CalculateAgreement(clientPublicKey);

            serverSalt = SaltHelper.ComputeSalt(newNonce, setClientDhParams.ServerNonce);
            
            return SerializeResponse(setClientDhParams, newNonce, serverAgree);
        }

        private static TDhGenOk SerializeResponse(RequestSetClientDHParams setClientDhParams, byte[] newNonce, BigInteger agreement)
        {
            var newNonceHash = Sha1Helper.ComputeHashsum(newNonce).Skip(4).ToList();
            
            var authKeyAuxHash = Sha1Helper.ComputeHashsum(agreement.ToByteArrayUnsigned()).Take(8).ToArray();

            newNonceHash.Add(1);
            newNonceHash.AddRange(authKeyAuxHash);
            
            return new TDhGenOk
                   {
                       Nonce = setClientDhParams.Nonce,
                       ServerNonce = setClientDhParams.ServerNonce,
                       NewNonceHash1 = newNonceHash.ToArray()
                   };
        }
        
        private static TClientDHInnerData DeserializeRequest(RequestSetClientDHParams serverDhParams, AesKeyData aesKeyData)
        {
            var encryptedAnswer = serverDhParams.EncryptedDataAsBinary;
            var answerWithHash = AES.DecryptAes(aesKeyData, encryptedAnswer);

            var serverHashsum = answerWithHash.Take(20).ToArray();
            var answer = answerWithHash.Skip(20).ToArray();

            var answerBuffer = PooledByteBufferAllocator.Default.Buffer();
            try
            {
                answerBuffer.WriteBytes(answer);
            
                var clientDhInnerData = Serializer.Deserialize(answerBuffer).Is<TClientDHInnerData>();

                return clientDhInnerData;

            }
            finally
            {
                answerBuffer.Release();
            }
        }
    }
}