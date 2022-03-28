using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public sealed class AuxiliaryBindingEditor : BindingEditor
    {
        public AuxiliaryBindingEditor(IControlBuilder controlBuilder)
        {
            var auxButtons = controlBuilder.Build<BindingDisplayList>();
            auxButtons.Prefix = "Auxiliary Binding";

            Content = new Scrollable
            {
                Border = BorderType.None,
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Spacing = 5,
                    Items =
                    {
                        new Group
                        {
                            Text = "Auxiliary",
                            Content = auxButtons
                        }
                    }
                }
            };

            auxButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettings>)c.AuxButtons));
        }
    }
}
