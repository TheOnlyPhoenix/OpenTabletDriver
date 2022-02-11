using System;

namespace OpenTabletDriver.Attributes.UI
{
    public class AspectRatioLockAttribute : Attribute
    {
        public AspectRatioLockAttribute(string targetMemberName, string settingMemberName)
        {
            TargetMemberName = targetMemberName;
            SettingMemberName = settingMemberName;
        }

        public string TargetMemberName { get; }
        public string SettingMemberName { get; }
    }
}
