using System;

#nullable enable

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Marks a property to be modified as a boolean.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingDescriptionAttribute : SettingAttribute
    {
        public SettingDescriptionAttribute(string displayName, string description) : base(displayName)
        {
            Description = description;
        }

        public string Description { get; }
    }
}
