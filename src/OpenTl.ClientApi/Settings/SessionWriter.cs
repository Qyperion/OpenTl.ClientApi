namespace OpenTl.ClientApi.Settings
{
    using System.Threading.Tasks;

    using Extensions;
    using MtProto;
    using Common.IoC;

    [SingleInstance(typeof(ISessionWriter))]
    internal class SessionWriter : ISessionWriter
    {
        public ISessionStore SessionStore { get; set; }

        public async Task Save(IClientSession clientSession)
        {
            await SessionStore.Save(clientSession.ToBytes());
        }
    }
}