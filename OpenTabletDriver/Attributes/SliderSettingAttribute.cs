using System;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Creates a slider for a property value between <see cref="Min"/> and <see cref="Max"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
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
