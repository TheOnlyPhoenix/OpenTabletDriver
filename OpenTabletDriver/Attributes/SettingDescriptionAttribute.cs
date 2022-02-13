using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Marks a property to be modified as a boolean.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class SettingDescriptionAttribute : SettingAttribute
    {
        public SettingDescriptionAttribute(string displayName, string description) : base(displayName)
        {
            Description = description;
        }

        public string Description { get; }
    }
}
