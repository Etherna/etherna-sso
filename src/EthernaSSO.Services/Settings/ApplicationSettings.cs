namespace Etherna.SSOServer.Services.Settings
{
    public class ApplicationSettings
    {
        string? _assemblyVersion;

        public string AssemblyVersion { get => _assemblyVersion ?? "1.0.0"; set => _assemblyVersion = value; }
        public string SimpleAssemblyVersion => AssemblyVersion.Split('+')[0];
    }
}
