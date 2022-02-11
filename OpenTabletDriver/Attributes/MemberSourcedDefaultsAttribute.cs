using System;

#nullable enable

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Applies the default value to a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MemberSourcedDefaultsAttribute : Attribute
    {
        public MemberSourcedDefaultsAttribute(string targetMemberName, params Type[] arguments)
        {
            TargetMemberName = targetMemberName;
        }

        public string TargetMemberName { get; }
    }
}
