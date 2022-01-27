using System;

namespace OpenTabletDriver.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class TabletReferenceAttribute : Attribute
    {
    }
}
