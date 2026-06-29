// Copyright 2021-present Etherna SA
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

namespace Etherna.SSOServer.Configs.Metrics
{
    using Prometheus;

    public static class SsoMetrics
    {
        // Fields.
        private static readonly Counter loginAttempts = Metrics.CreateCounter(
            "sso_login_attempts_total",
            "Total login attempts.",
            new CounterConfiguration
            {
                LabelNames = ["method", "result"]
            });

        private static readonly Counter registrations = Metrics.CreateCounter(
            "sso_account_registrations_total",
            "Total account registrations.",
            new CounterConfiguration
            {
                LabelNames = ["method"]
            });

        private static readonly Counter clientAppsCreated = Metrics.CreateCounter(
            "sso_client_apps_created_total",
            "Total OAuth client applications created by developers.",
            new CounterConfiguration
            {
                LabelNames = ["client_type"]
            });

        // Methods.
        public static void RecordLoginAttempt(string method, string result) =>
            loginAttempts.WithLabels(method, result).Inc();

        public static void RecordRegistration(string method) =>
            registrations.WithLabels(method).Inc();

        public static void RecordClientAppCreated(string clientType) =>
            clientAppsCreated.WithLabels(clientType).Inc();
    }
}
