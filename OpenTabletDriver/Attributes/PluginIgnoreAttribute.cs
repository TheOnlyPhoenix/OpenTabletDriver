using System;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Marks a plugin class to be ignored in reflection calls.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class PluginIgnoreAttribute : Attribute
    {
    }
}
