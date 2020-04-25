namespace Etherna.SSOServer.Services.Settings
{
    public class EmailSettings
    {
        public string DisplayName { get; set; } = default!;
        public string SendingAddress { get; set; } = default!;
        public string ServiceKey { get; set; } = default!;
        public string ServiceUser { get; set; } = default!;
    }
}
