namespace OpenTl.Common.Auth
{
    using System;

    using BarsGroup.CodeGuard;

    using OpenTl.Common.Crypto;

    public sealed class AuthKey
    {
        private readonly ulong _auxHash;

        public AuthKey(byte[] data)
        {
            Guard.That(data.Length, nameof(data)).IsEqual(256);

            Data = data;

            var hashsum = Sha1Helper.ComputeHashsum(data);
            _auxHash = BitConverter.ToUInt64(hashsum, 0);
            Id = BitConverter.ToInt64(hashsum, 8 + 4);
        }

        public byte[] Data { get; }

        public long Id { get; }

        public Guid ToGuid()
        {
            var guid = new byte[16];
            BitConverter.GetBytes(Id).CopyTo(guid, 8);
            
            return new Guid(guid);
        }
    }
}