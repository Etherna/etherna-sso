namespace Etherna.SSOServer.Persistence
{
    public class PersistenceOptions
    {
        public PersistenceOptions()
        {
            MongODM = new MongOdmOptions();
        }

        public MongOdmOptions MongODM { get; set; }
    }
}
