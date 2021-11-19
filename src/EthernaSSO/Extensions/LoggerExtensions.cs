//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Etherna.SSOServer.Extensions
{
    /*
     * Always group similar log delegates by type, always use incremental event ids.
     * Last event id is: 19
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

        private static readonly Action<ILogger, string, Exception> _passwordChanged =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(10, nameof(PasswordChanged)),
                "User with ID '{UserId}' changed its password successfully.");

        private static readonly Action<ILogger, string, Exception> _resetted2FAAuthApp =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(16, nameof(Resetted2FAAuthApp)),
                "User with ID '{UserId}' has reset their authentication app key.");

        //*** WARNING LOGS ***
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
                "User with ID '{UserId}' account locked out login attempt.");

        // Methods.
        public static void AccountDeleted(this ILogger logger, string userId, string deletedByUserId) =>
            _accountDeleted(logger, userId, deletedByUserId, null!);

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

        public static void PasswordChanged(this ILogger logger, string userId) =>
            _passwordChanged(logger, userId, null!);

        public static void Resetted2FAAuthApp(this ILogger logger, string userId) =>
            _resetted2FAAuthApp(logger, userId, null!);
    }
}
