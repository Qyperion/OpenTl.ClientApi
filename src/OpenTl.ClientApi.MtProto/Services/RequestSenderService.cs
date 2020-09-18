namespace OpenTl.ClientApi.MtProto.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    using NullGuard;

    using Extensions;
    using OpenTl.ClientApi.MtProto.Interfaces;
    using Interfaces;
    using Common.IoC;
    using Schema;

    [SingleInstance(typeof(IRequestSender))]
    internal class RequestSenderService: IRequestSender
    {
        public IRequestService RequestService { get; set; }

        public IClientSettings ClientSettings { get; set; }

        public IContextGetter ContextGetter { get; set; }

        /// <inheritdoc />
        [return: AllowNull]
        public async Task<TResult> SendRequestAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken)
        {
            var resultTask = RequestService.RegisterRequest(request, cancellationToken);

            if (ClientSettings.ConnectionWasInitialize())
            {
                await ContextGetter.Context.WriteAndFlushAsync(request);
            }

            return (TResult)await resultTask;
        }
    }
}