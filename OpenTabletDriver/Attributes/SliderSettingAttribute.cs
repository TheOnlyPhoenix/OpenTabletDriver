using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Creates a slider for a property value between <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public class SliderSettingAttribute : SettingAttribute
    {
        public SliderSettingAttribute(string displayName, float min, float max, float defaultValue = 0f) : base(displayName)
        {
            Min = min;
            Max = max;
            DefaultValue = defaultValue;
        }

        public float Min { get; }
        public float Max { get; }
        public float DefaultValue { get; }
    }
}
