using System.Reflection;

#nullable enable

namespace OpenTabletDriver.Desktop.Reflection
{
    public interface IPluginFactory
    {
        T? Construct<T>(string name, params object[] args) where T : class;
        TypeInfo? GetPluginType(string name);
    }
}
