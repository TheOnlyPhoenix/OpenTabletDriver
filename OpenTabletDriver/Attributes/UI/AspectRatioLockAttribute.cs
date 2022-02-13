using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes.UI
{
    /// <summary>
    /// Designates aspect ratio locking between area properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class AspectRatioLockAttribute : Attribute
    {
        public AspectRatioLockAttribute(string targetMemberName, string settingMemberName)
        {
            TargetMemberName = targetMemberName;
            SettingMemberName = settingMemberName;
        }

        /// <summary>
        /// The target area setting to lock aspect ratio to.
        /// </summary>
        public string TargetMemberName { get; }

        /// <summary>
        /// The setting determining whether aspect ratio locking will occur.
        /// </summary>
        public string SettingMemberName { get; }
    }
}
