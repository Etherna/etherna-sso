namespace Etherna.SSOServer.Services.Settings
{
    public class ApplicationSettings
    {
        public string AssemblyVersion { get; set; } = default!;
        public string SimpleAssemblyVersion => AssemblyVersion.Split('+')[0];
    }
}
