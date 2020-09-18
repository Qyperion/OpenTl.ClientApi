using System.Linq;
using BarsGroup.CodeGuard;
using OpenTl.Common.Crypto;
using OpenTl.Common.Interfaces;

namespace OpenTl.Common.MtProto
{
    using System.Security.Cryptography;

    using DotNetty.Buffers;
    using DotNetty.Common.Utilities;

    using OpenTl.Common.Extensions;

    using Org.BouncyCastle.Security;

    public static class MtProtoHelper
    {
        private static readonly SecureRandom Random = new SecureRandom();

        public static AesKeyData CalcKey(byte[] authKey, byte[] msgKey, bool toServer)
        {
            Guard.That(authKey.Length, nameof(authKey)).IsEqual(256);
            Guard.That(msgKey.Length, nameof(msgKey)).IsEqual(16);

            var x = toServer
                        ? 0
                        : 8;

            //sha256_a = SHA256 (msg_key + substr (auth_key, x, 36));
            var sha256ASource = msgKey.Concat(authKey.Skip(x).Take(36)).ToArray();
            var sha256A = Sha256(sha256ASource);

            //sha256_b = SHA256 (substr (auth_key, 40+x, 36) + msg_key);
            var sha256BSource = authKey.Skip(40 + x).Take(36).Concat(msgKey).ToArray();
            var sha256B = Sha256(sha256BSource);

            //aes_key = substr (sha256_a, 0, 8) + substr (sha256_b, 8, 16) + substr (sha256_a, 24, 8);
            var aesKey = sha256A.Take(8).Concat(sha256B.Skip(8).Take(16)).Concat(sha256A.Skip(24).Take(8)).ToArray();

            //aes_iv = substr (sha256_b, 0, 8) + substr (sha256_a, 8, 16) + substr (sha256_b, 24, 8);
            var aesIv = sha256B.Take(8).Concat(sha256A.Skip(8).Take(16)).Concat(sha256B.Skip(24).Take(8)).ToArray();

            return new AesKeyData(aesKey, aesIv);
        }

        private static byte[] CalcMsgKey(byte[] authKey, byte[] data)
        {
            //msg_key_large = SHA256 (substr (auth_key, 88+0, 32) + plaintext + random_padding);
            var msgKeyLarge = Sha256(authKey.Skip(88).Take(32).Concat(data).ToArray());

            //msg_key = substr (msg_key_large, 8, 16);
            return msgKeyLarge.Skip(8).Take(16).ToArray();
        }

        private static byte[] Sha256(byte[] data)
        {
            using (var sha1 = SHA256.Create())
            {
                return sha1.ComputeHash(data);
            }
        }

        public static void ToServerEncrypt(IByteBuffer packet, ISession session, long messageId, int seqNumber, IByteBuffer output)
        {
            Encrypt(packet, true, session, messageId, seqNumber, output);
        }

        public static IByteBuffer FromClientDecrypt(IByteBuffer packet,
                                                    ISession session,
                                                    out long authKeyId,
                                                    out byte[] serverSalt,
                                                    out ulong sessionId,
                                                    out long messageId,
                                                    out int seqNumber)
        {
            return Decrypt(packet, session, true, out authKeyId, out serverSalt, out sessionId, out messageId, out seqNumber);
        }

        public static void ToClientEncrypt(IByteBuffer packet, ISession session, long messageId, int seqNumber, IByteBuffer output)
        {
            Encrypt(packet, false, session, messageId, seqNumber, output);
        }

        public static IByteBuffer FromServerDecrypt(IByteBuffer packet,
                                                    ISession session,
                                                    out long authKeyId,
                                                    out byte[] serverSalt,
                                                    out ulong sessionId,
                                                    out long messageId,
                                                    out int seqNumber)
        {
            return Decrypt(packet, session, false, out authKeyId, out serverSalt, out sessionId, out messageId, out seqNumber);
        }

        private static IByteBuffer Decrypt(IByteBuffer packet,
                                           ISession session,
                                           bool toServer,
                                           out long authKeyId,
                                           out byte[] serverSalt,
                                           out ulong sessionId,
                                           out long messageId,
                                           out int seqNumber)
        {
            authKeyId = packet.ReadLongLE();
            var messageKey = packet.ToArray(16);
            var encryptedData = packet.ToArray(packet.ReadableBytes);

            var aesKey = CalcKey(session.AuthKey.Data, messageKey, toServer);

            var message = AES.DecryptAes(aesKey, encryptedData);

            var messageBuffer = PooledByteBufferAllocator.Default.Buffer(message.Length);
            try
            {
                messageBuffer.WriteBytes(message);

                serverSalt = messageBuffer.ToArray(8);
                sessionId = (ulong)messageBuffer.ReadLongLE();
                messageId = messageBuffer.ReadLongLE();
                seqNumber = messageBuffer.ReadIntLE();
                var length = messageBuffer.ReadIntLE();

                return messageBuffer.ReadBytes(length);
            }
            finally
            {
                messageBuffer.SafeRelease();
            }
        }

        private static void Encrypt(IByteBuffer inputBuffer, bool toServer, ISession session, long messageId, int seqNumber, IByteBuffer output)
        {

            var messageBuffer = PooledByteBufferAllocator.Default.Buffer();

            byte[] messageData;
            try
            {
                messageBuffer.WriteBytes(session.ServerSalt);
                messageBuffer.WriteLongLE((long)session.SessionId);
                messageBuffer.WriteLongLE(messageId);
                messageBuffer.WriteIntLE(seqNumber);

                messageBuffer.WriteIntLE(inputBuffer.ReadableBytes);
                messageBuffer.WriteBytes(inputBuffer);

                var randomPaddingLenght = Random.Next(1024 / 16) * 16 + 16 - messageBuffer.ReadableBytes % 16;
                messageBuffer.WriteBytes(Random.GenerateSeed(randomPaddingLenght));

                messageData = messageBuffer.ToArray();
            }
            finally
            {
                messageBuffer.SafeRelease();
            }

            var messageKey = CalcMsgKey(session.AuthKey.Data, messageData);

            var aesKey = CalcKey(session.AuthKey.Data, messageKey, toServer);

            output.WriteLongLE((long)session.AuthKey.Id);
            output.WriteBytes(messageKey);
            output.WriteBytes(AES.EncryptAes(aesKey, messageData));
        }
    }
}