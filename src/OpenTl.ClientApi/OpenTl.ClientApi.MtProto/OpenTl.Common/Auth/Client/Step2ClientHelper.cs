namespace OpenTl.Common.Auth.Client
{
    using System;
    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    using Crypto;
    using Extensions;
    using Schema;
    using Schema.Serialization;

    using Org.BouncyCastle.Math;

    public static class Step2ClientHelper
    {
        private static readonly Random Random = new Random();

        public static RequestReqDHParams GetRequest(TResPQ resPq, string publicKey, out byte[] newNonce)
        {
            var pq = new BigInteger(resPq.PqAsBinary);
            var f1 = PollardRho.Factor(pq);
            var f2 = pq.Divide(f1);
            var p = f1.Min(f2);
            var q = f1.Max(f2);
            
            newNonce = new byte[32];
            Random.NextBytes(newNonce);

            var pqInnerData = new TPQInnerData
                              {
                                  PqAsBinary = pq.ToByteArrayUnsigned(),
                                  PAsBinary = p.ToByteArrayUnsigned(),
                                  QAsBinary = q.ToByteArrayUnsigned(),
                                  ServerNonce = resPq.ServerNonce,
                                  Nonce = resPq.Nonce,
                                  NewNonce = newNonce
                              };

            var serializedData = Serializer.Serialize(pqInnerData);

            byte[] innerData;
            try
            {
                innerData = serializedData.ToArray();
            }
            finally
            {
                serializedData.SafeRelease();
            }

            var fingerprint = RSAHelper.GetFingerprint(publicKey);
            if (!resPq.ServerPublicKeyFingerprints.Contains(fingerprint))
            {
                 throw new InvalidOperationException("The fingerprint is not found");
            }
            
            var hashsum = Sha1Helper.ComputeHashsum(innerData);

            var dataWithHash = PooledByteBufferAllocator.Default.Buffer();

            byte[] innerDataWithHash;
            try
            {
                dataWithHash.WriteBytes(hashsum);
                dataWithHash.WriteBytes(innerData);
            
                var paddingBytes = new byte[255 - dataWithHash.ReadableBytes];
                Random.NextBytes(paddingBytes);
                dataWithHash.WriteBytes(paddingBytes);
                innerDataWithHash = dataWithHash.ToArray();
            }
            finally
            {
                dataWithHash.SafeRelease();
            }
            
            var ciphertext = RSAHelper.RsaEncryptWithPublic(innerDataWithHash, publicKey);

            return new RequestReqDHParams
                   {
                       Nonce = resPq.Nonce,
                       PAsBinary = p.ToByteArrayUnsigned(),
                       QAsBinary = q.ToByteArrayUnsigned(),
                       ServerNonce = resPq.ServerNonce,
                       PublicKeyFingerprint = fingerprint,
                       EncryptedDataAsBinary = ciphertext
                   };
        }
    }
}