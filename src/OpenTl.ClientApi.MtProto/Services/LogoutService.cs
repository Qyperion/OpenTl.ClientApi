﻿namespace OpenTl.ClientApi.MtProto.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    using Exceptions;
    using OpenTl.ClientApi.MtProto.Interfaces;
    using Interfaces;
    using Common.IoC;
    using Schema.Auth;

    [SingleInstance(typeof(ILogoutService))]
    internal class LogoutService: ILogoutService
    {
        public IClientSettings ClientSettings { get; set; }

        public IContextGetter ContextGetter { get; set; }

        public ISessionWriter SessionWriter { get; set; }

        public IRequestService RequestService { get; set; }

        /// <inheritdoc />
        public async Task Logout(CancellationToken cancellationToken)
        {
            if (ClientSettings.ClientSession.UserId.HasValue)
            {
                await ContextGetter.Context.WriteAndFlushAsync(new RequestLogOut());

                RequestService.ReturnException(new UserLogoutException());

                ClientSettings.ClientSession.UserId = null;
                
                await SessionWriter.Save(ClientSettings.ClientSession);
            }
        }
    }
}