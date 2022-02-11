using System;
using System.Collections.Generic;
using System.Reflection;

#nullable enable

namespace OpenTabletDriver.Desktop.Reflection
{
    public interface IPluginFactory
    {
        T? Construct<T>(PluginSettings settings, params object[] args) where T : class;
        T? Construct<T>(string fullPath, params object[] args) where T : class;
        TypeInfo? GetPluginType(string path);
        IEnumerable<TypeInfo> GetMatchingTypes(Type baseType);
        string? GetFriendlyName(string path);
    }
}
