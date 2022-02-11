using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Marks a property to be modified and saved by a client to settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign | ImplicitUseKindFlags.Access)]
    public class SettingAttribute : Attribute
    {
        public SettingAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }
    }
}
