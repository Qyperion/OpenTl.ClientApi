using OpenTl.Common.Auth;

namespace OpenTl.Common.Interfaces
{
    public interface ISession
    {
        AuthKey AuthKey { get; set; }
        
        ulong SessionId { get; }
        
        int LastMessageId { get; set; }
        
        int SequenceNumber { get; set; }
        
        byte[] ServerSalt { get; set; }

        int? UserId { get; set; }
    }
}