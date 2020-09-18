﻿namespace OpenTl.ClientApi.Extensions
{
    using MtProto;
    using MtProto.Exceptions;

    internal static class SettingsExtensions
    {
        public static void EnsureUserAuthorized(this IClientSettings clientSession)
        {
            if (!clientSession.IsUserAuthorized())
            {
                throw new UserNotAuthorizeException();
            }
        }

        public static bool IsUserAuthorized(this IClientSettings clientSession)
        {
            return clientSession.ClientSession.UserId.HasValue;
        }
    }
}