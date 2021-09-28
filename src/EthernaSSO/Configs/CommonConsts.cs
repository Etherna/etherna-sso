namespace Etherna.SSOServer.Configs
{
    public static class CommonConsts
    {
        public const string AdminArea = "Admin";
        public const string IdentityArea = "Identity";

        public const string DatabaseAdminPath = "/admin/db";
        public const string HangfireAdminPath = "/admin/hangfire";

        public const string RequireAdministratorRolePolicy = "RequireAdministratorRole";
        public const string ServiceInteractApiScopePolicy = "ServiceInteractApiScope";
    }
}
