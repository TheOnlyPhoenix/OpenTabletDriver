namespace OpenTabletDriver.Platform.Environment
{
    public interface IEnvironmentHandler
    {
        void Open(string path);
        void OpenFolder(string path);
    }
}
