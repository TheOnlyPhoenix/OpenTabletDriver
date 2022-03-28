using System.Collections.Generic;
using Eto.Forms;
using JetBrains.Annotations;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public class MouseBindingEditor : BindingEditor
    {
        public MouseBindingEditor(IControlBuilder controlBuilder)
        {
            var scrollDown = controlBuilder.Build<BindingDisplay>();
            var scrollUp = controlBuilder.Build<BindingDisplay>();

            var mouseButtons = controlBuilder.Build<MouseBindingDisplayList>();
            mouseButtons.Prefix = "Mouse Binding";

            Content = new Scrollable
            {
                Border = BorderType.None,
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items =
                    {
                        new Group
                        {
                            Text = "Mouse Buttons",
                            Content = mouseButtons
                        },
                        new Group
                        {
                            Text = "Mouse Scrollwheel",
                            Content = new StackLayout
                            {
                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                Spacing = 5,
                                Items =
                                {
                                    new Group
                                    {
                                        Text = "Scroll Up",
                                        Orientation = Orientation.Horizontal,
                                        ExpandContent = false,
                                        Content = scrollUp
                                    },
                                    new Group
                                    {
                                        Text = "Scroll Down",
                                        Orientation = Orientation.Horizontal,
                                        ExpandContent = false,
                                        Content = scrollDown
                                    }
                                }
                            }
                        }
                    }
                }
            };

            mouseButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettings>)c.MouseButtons));
            scrollUp.StoreBinding.Bind(SettingsBinding.Child(c => c.MouseScrollUp));
            scrollDown.StoreBinding.Bind(SettingsBinding.Child(c => c.MouseScrollDown));
        }

        private class MouseBindingDisplayList : BindingDisplayList
        {
            public MouseBindingDisplayList(IControlBuilder controlBuilder) : base(controlBuilder)
            {
            }

            protected override string GetTextForIndex(int index)
            {
                return index switch
                {
                    0 => "Primary Binding",
                    1 => "Alternate Binding",
                    2 => "Middle Binding",
                    _ => base.GetTextForIndex(index)
                };
            }
        }
    }
}
