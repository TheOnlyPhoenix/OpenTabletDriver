using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.UX.Controls
{
    public interface IControlGenerator
    {
        Control Generate(TypeInfo type, DirectBinding<PluginSettings> settings);
        Control GetControlForSetting(PropertyInfo property, SettingAttribute attribute, DirectBinding<PluginSetting> binding);
    }
}
