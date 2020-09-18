namespace OpenTl.ClientApi.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    using BarsGroup.CodeGuard;
    using MtProto;
    using Interfaces;
    using Common.IoC;
    using Schema;
    using Schema.Auth;

    using TAuthorization = Schema.Auth.TAuthorization;

    [SingleInstance(typeof(IAuthService))]
    internal sealed class AuthService : IAuthService
    {
        public IRequestSender RequestSender { get; set; }

        public IClientSettings ClientSettings { get; set; }

        public ISessionWriter SessionWriter { get; set; }

        public ILogoutService LogoutService { get; set; }
        
        /// <inheritdoc />
        public int? CurrentUserId => ClientSettings.ClientSession.UserId;

        /// <inheritdoc />
        public async Task<ISentCode> SendCodeAsync(string phoneNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.That(phoneNumber, nameof(phoneNumber)).IsNotNullOrWhiteSpace();

            var request = new RequestSendCode
                          {
                              PhoneNumber = phoneNumber,
                              ApiId = ClientSettings.AppId,
                              ApiHash = ClientSettings.AppHash,
                          };

            return await RequestSender.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task LogoutAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await LogoutService.Logout(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TUser> SignInAsync(string phoneNumber, ISentCode sentCode, string code, CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.That(phoneNumber, nameof(phoneNumber)).IsNotNullOrWhiteSpace();
            Guard.That(code, nameof(code)).IsNotNullOrWhiteSpace();

            var request = new RequestSignIn
                          {
                              PhoneNumber = phoneNumber,
                              PhoneCodeHash = sentCode.PhoneCodeHash,
                              PhoneCode = code
                          };

            var result = (TAuthorization)await RequestSender.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            var user = result.User.Is<TUser>();

            await OnUserAuthenticated(user).ConfigureAwait(false);

            return user;
        }

        /// <inheritdoc />
        public async Task<TUser> SignUpAsync(string phoneNumber,
                                             ISentCode sentCode,
                                             string code,
                                             string firstName,
                                             string lastName,
                                             CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.That(phoneNumber, nameof(phoneNumber)).IsNotNullOrWhiteSpace();
            Guard.That(code, nameof(code)).IsNotNullOrWhiteSpace();
            Guard.That(firstName, nameof(firstName)).IsNotNullOrWhiteSpace();
            Guard.That(lastName, nameof(lastName)).IsNotNullOrWhiteSpace();

            var request = new RequestSignUp
                          {
                              PhoneNumber = phoneNumber,
                              PhoneCode = code,
                              PhoneCodeHash = sentCode.PhoneCodeHash,
                              FirstName = firstName,
                              LastName = lastName
                          };
            var result = (TAuthorization)await RequestSender.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            var user = result.User.Is<TUser>();

            await OnUserAuthenticated(user).ConfigureAwait(false);
            return user;
        }
        

        private async Task OnUserAuthenticated(TUser user)
        {
            var session = ClientSettings.ClientSession;

            session.UserId = user.Id;

            await SessionWriter.Save(session).ConfigureAwait(false);
        }
    }
}