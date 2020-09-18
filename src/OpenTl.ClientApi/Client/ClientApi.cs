﻿namespace OpenTl.ClientApi.Client
{
    using System;
    using System.Threading;

    using Castle.Windsor;

    using OpenTl.ClientApi.Services.Interfaces;
    using OpenTl.Common.Extensions;
    using Common.IoC;
    using Schema;

    /// <inheritdoc />
    [SingleInstance(typeof(IClientApi))]
    internal class ClientApi : IClientApi
    {
        private Timer _keepAliveTimer;
        
        public IWindsorContainer Container { get; set; }

        /// <inheritdoc />
        public IUpdatesService UpdatesService { get; set; }

        /// <inheritdoc />
        public ICustomRequestsService CustomRequestsService { get; set; }

        /// <inheritdoc />
        public IAuthService AuthService { get; set; }

        /// <inheritdoc />
        public IContactsService ContactsService { get; set; }

        /// <inheritdoc />
        public IUsersService UsersService { get; set; }

        /// <inheritdoc />
        public IMessagesService MessagesService { get; set; }

        /// <inheritdoc />
        public IFileService FileService { get; set; }

        /// <inheritdoc />
        public IHelpService HelpService { get; set; }

        public void Dispose()
        {
            Container?.Dispose();
        }
        
        
        /// <inheritdoc />
        public void KeepAliveConnection()
        {
            _keepAliveTimer?.Dispose();
            
            _keepAliveTimer = new Timer(
                _ =>
                {
                    var requestPing = new RequestPing { PingId = new Random().NextLong() };

                    CustomRequestsService.SendRequestAsync(requestPing, CancellationToken.None).ConfigureAwait(false);
                },
                null,
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(1));
        }
    }
}