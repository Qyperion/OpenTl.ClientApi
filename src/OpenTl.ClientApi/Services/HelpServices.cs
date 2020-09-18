namespace OpenTl.ClientApi.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    using MtProto;
    using Interfaces;
    using Common.IoC;
    using Schema;
    using Schema.Help;

    /// <inheritdoc />
    [SingleInstance(typeof(IHelpService))]
    internal class HelpService : IHelpService
    {
        public IRequestSender RequestSender { get; set; }

        /// <inheritdoc />
        public async Task<TConfig> GetConfig(CancellationToken cancellationToken = default(CancellationToken))
        {
            return (TConfig)await RequestSender.SendRequestAsync(new RequestGetConfig(), cancellationToken).ConfigureAwait(false);
        }
    }
}