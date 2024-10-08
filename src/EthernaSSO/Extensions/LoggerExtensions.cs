﻿// Copyright 2021-present Etherna SA
// This file is part of Etherna Sso.
// 
// Etherna Sso is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Sso is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Sso.
// If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 26
     */
    public static class LoggerExtensions
    {
        // Fields.
        //*** DEBUG LOGS ***
        private static readonly Action<ILogger, IEnumerable<string>, Exception> _externalClaims =
            LoggerMessage.Define<IEnumerable<string>>(
                LogLevel.Debug,
                new EventId(0, nameof(ExternalClaims)),
                "External claims: {Claims}");

        //*** INFORMATION LOGS ***
        private static readonly Action<ILogger, string, string, Exception> _accountDeleted =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(11, nameof(AccountDeleted)),
                "Account of user with ID '{UserId}' has been deleted by user with id {DeleterUserId}.");

        private static readonly Action<ILogger, string, Exception> _apiKeyValidatedLoginAttempt =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(24, nameof(ApiKeyValidatedLoginAttempt)),
                "User with ID '{UserId}' provided valid api key during login attempt.");

        private static readonly Action<ILogger, string, Exception> _createdAccountWithPassword =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(17, nameof(CreatedAccountWithPassword)),
                "User with ID '{UserId}' created a new account with password.");

        private static readonly Action<ILogger, string, string, Exception> _createdAccountWithProvider =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(2, nameof(CreatedAccountWithProvider)),
                "User with ID '{UserId}' created an account using {LoginProvider} provider.");

        private static readonly Action<ILogger, string, Exception> _createdAccountWithWeb3 =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(19, nameof(CreatedAccountWithWeb3)),
                "User with ID '{UserId}' created a web3 account.");

        private static readonly Action<ILogger, string, Exception> _disabled2FA =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(12, nameof(Disabled2FA)),
                "User with ID '{UserId}' has disabled 2fa.");

        private static readonly Action<ILogger, string, Exception> _downloadedPersonalData =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(13, nameof(DownloadedPersonalData)),
                "User with ID '{UserId}' asked for their personal data.");

        private static readonly Action<ILogger, string, Exception> _enabled2FAWithAuthApp =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(14, nameof(Enabled2FAWithAuthApp)),
                "User with ID '{UserId}' has enabled 2FA with an authenticator app.");

        private static readonly Action<ILogger, string, Exception> _generated2FARecoveryCodes =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(15, nameof(Generated2FARecoveryCodes)),
                "User with ID '{UserId}' has generated new 2FA recovery codes.");

        private static readonly Action<ILogger, Exception> _loggedOut =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(9, nameof(LoggedOut)),
                "User logged out.");

        private static readonly Action<ILogger, string, Exception> _loggedInWith2FA =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(5, nameof(LoggedInWith2FA)),
                "User with ID '{UserId}' logged in with 2fa.");

        private static readonly Action<ILogger, string, Exception> _loggedInWithPassword =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(3, nameof(LoggedInWithPassword)),
                "User with ID '{UserId}' logged in with password.");

        private static readonly Action<ILogger, string, string, Exception> _loggedInWithProvider =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(1, nameof(LoggedInWithProvider)),
                "{Name} logged in with {LoginProvider} provider.");

        private static readonly Action<ILogger, string, Exception> _loggedInWithRecoveryCode =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(7, nameof(LoggedInWithRecoveryCode)),
                "User with ID '{UserId}' logged in with a recovery code.");

        private static readonly Action<ILogger, string, Exception> _loggedInWithWeb3 =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(18, nameof(LoggedInWithWeb3)),
                "User with ID '{UserId}' logged in with web3.");

        private static readonly Action<ILogger, string, Exception> _noUserFoundMatchingUsername =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(20, nameof(NoUserFoundMatchingUsername)),
                "No user found matching username: {Username}");

        private static readonly Action<ILogger, string, Exception> _noUserFoundWithId =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(25, nameof(NoUserFoundWithId)),
                "No user found with Id: {UserId}");

        private static readonly Action<ILogger, string, Exception> _passwordChanged =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(10, nameof(PasswordChanged)),
                "User with ID '{UserId}' changed its password successfully.");

        private static readonly Action<ILogger, string, Exception> _refreshedLogin =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(26, nameof(RefreshedLogin)),
                "User with ID '{UserId}' refreshed login.");

        private static readonly Action<ILogger, string, Exception> _resetted2FAAuthApp =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(16, nameof(Resetted2FAAuthApp)),
                "User with ID '{UserId}' has reset their authentication app key.");

        //*** WARNING LOGS ***
        private static readonly Action<ILogger, string, Exception> _apiKeyDoesNotExistLoginAttempt =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(22, nameof(ApiKeyDoesNotExistLoginAttempt)),
                "User with ID '{UserId}' failed login attempt because api key does not exist.");

        private static readonly Action<ILogger, string, Exception> _apiKeyIsNotAliveLoginAttempt =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(23, nameof(ApiKeyIsNotAliveLoginAttempt)),
                "User with ID '{UserId}' failed login attempt because api key is not alive.");

        private static readonly Action<ILogger, string, Exception> _invalid2FACodeAttempt =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(6, nameof(Invalid2FACodeAttempt)),
                "Invalid authenticator code entered for user with ID '{UserId}'.");

        private static readonly Action<ILogger, string, Exception> _invalidRecoveryCodeAttempt =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(8, nameof(InvalidRecoveryCodeAttempt)),
                "Invalid recovery code entered for user with ID '{UserId}'.");

        private static readonly Action<ILogger, string, Exception> _lockedOutLoginAttempt =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(4, nameof(LockedOutLoginAttempt)),
                "User with ID '{UserId}' failed login attempt because account is locked out.");

        private static readonly Action<ILogger, string, Exception> _notAllowedSingInLoginAttempt =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(21, nameof(NotAllowedSingInLoginAttempt)),
                "User with ID '{UserId}' failed login attempt because account is not allowed to sign in.");

        // Methods.
        public static void AccountDeleted(this ILogger logger, string userId, string deletedByUserId) =>
            _accountDeleted(logger, userId, deletedByUserId, null!);

        public static void ApiKeyDoesNotExistLoginAttempt(this ILogger logger, string userId) =>
            _apiKeyDoesNotExistLoginAttempt(logger, userId, null!);

        public static void ApiKeyIsNotAliveLoginAttempt(this ILogger logger, string userId) =>
            _apiKeyIsNotAliveLoginAttempt(logger, userId, null!);

        public static void ApiKeyValidatedLoginAttempt(this ILogger logger, string userId) =>
            _apiKeyValidatedLoginAttempt(logger, userId, null!);

        public static void CreatedAccountWithPassword(this ILogger logger, string userId) =>
            _createdAccountWithPassword(logger, userId, null!);

        public static void CreatedAccountWithProvider(this ILogger logger, string userId, string loginProvider) =>
            _createdAccountWithProvider(logger, userId, loginProvider, null!);

        public static void CreatedAccountWithWeb3(this ILogger logger, string userId) =>
            _createdAccountWithWeb3(logger, userId, null!);

        public static void Disabled2FA(this ILogger logger, string userId) =>
            _disabled2FA(logger, userId, null!);

        public static void DownloadedPersonalData(this ILogger logger, string userId) =>
            _downloadedPersonalData(logger, userId, null!);

        public static void Enabled2FAWithAuthApp(this ILogger logger, string userId) =>
            _enabled2FAWithAuthApp(logger, userId, null!);

        public static void ExternalClaims(this ILogger logger, IEnumerable<string> claims) =>
            _externalClaims(logger, claims, null!);

        public static void Generated2FARecoveryCodes(this ILogger logger, string userId) =>
            _generated2FARecoveryCodes(logger, userId, null!);

        public static void Invalid2FACodeAttempt(this ILogger logger, string userId) =>
            _invalid2FACodeAttempt(logger, userId, null!);

        public static void InvalidRecoveryCodeAttempt(this ILogger logger, string userId) =>
            _invalidRecoveryCodeAttempt(logger, userId, null!);

        public static void LockedOutLoginAttempt(this ILogger logger, string userId) =>
            _lockedOutLoginAttempt(logger, userId, null!);

        public static void LoggedOut(this ILogger logger) =>
            _loggedOut(logger, null!);

        public static void LoggedInWith2FA(this ILogger logger, string userId) =>
            _loggedInWith2FA(logger, userId, null!);

        public static void LoggedInWithPassword(this ILogger logger, string userId) =>
            _loggedInWithPassword(logger, userId, null!);

        public static void LoggedInWithProvider(this ILogger logger, string name, string loginProvider) =>
            _loggedInWithProvider(logger, name, loginProvider, null!);

        public static void LoggedInWithRecoveryCode(this ILogger logger, string userId) =>
            _loggedInWithRecoveryCode(logger, userId, null!);

        public static void LoggedInWithWeb3(this ILogger logger, string userId) =>
            _loggedInWithWeb3(logger, userId, null!);

        public static void NotAllowedSingInLoginAttempt(this ILogger logger, string userId) =>
            _notAllowedSingInLoginAttempt(logger, userId, null!);

        public static void NoUserFoundMatchingUsername(this ILogger logger, string username) =>
            _noUserFoundMatchingUsername(logger, username, null!);

        public static void NoUserFoundWithId(this ILogger logger, string userId) =>
            _noUserFoundWithId(logger, userId, null!);

        public static void PasswordChanged(this ILogger logger, string userId) =>
            _passwordChanged(logger, userId, null!);

        public static void RefreshedLogin(this ILogger logger, string userId) =>
            _refreshedLogin(logger, userId, null!);

        public static void Resetted2FAAuthApp(this ILogger logger, string userId) =>
            _resetted2FAAuthApp(logger, userId, null!);
    }
}
