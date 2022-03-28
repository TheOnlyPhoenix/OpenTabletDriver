using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public sealed class PenBindingEditor : BindingEditor
    {
        public PenBindingEditor(IControlBuilder controlBuilder)
        {
            _penButtons = controlBuilder.Build<BindingDisplayList>();
            _penButtons.Prefix = "Pen Button";

            Content = new Scrollable
            {
                Border = BorderType.None,
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items =
                    {
                        new TableLayout
                        {
                            Rows =
                            {
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Group
                                        {
                                            Text = "Tip Settings",
                                            Content = new StackLayout
                                            {
                                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                                Spacing = 5,
                                                Items =
                                                {
                                                    new Group
                                                    {
                                                        Text = "Tip Binding",
                                                        Orientation = Orientation.Horizontal,
                                                        ExpandContent = false,
                                                        Content = _tipButton = controlBuilder.Build<BindingDisplay>()
                                                    },
                                                    new Group
                                                    {
                                                        Text = "Tip Threshold",
                                                        ToolTip = "The minimum threshold in order for the assigned binding to activate.",
                                                        Orientation = Orientation.Horizontal,
                                                        Content = _tipThreshold = new FloatSlider()
                                                    }
                                                }
                                            }
                                        },
                                        new Group
                                        {
                                            Text = "Eraser Settings",
                                            Content = new StackLayout
                                            {
                                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                                Spacing = 5,
                                                Items =
                                                {
                                                    new Group
                                                    {
                                                        Text = "Eraser Binding",
                                                        ExpandContent = false,
                                                        Orientation = Orientation.Horizontal,
                                                        Content = _eraserButton = controlBuilder.Build<BindingDisplay>()
                                                    },
                                                    new Group
                                                    {
                                                        Text = "Eraser Threshold",
                                                        ToolTip = "The minimum threshold in order for the assigned binding to activate.",
                                                        Orientation = Orientation.Horizontal,
                                                        Content = _eraserThreshold = new FloatSlider()
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Group
                        {
                            Text = "Pen Buttons",
                            Content = _penButtons
                        }
                    }
                }
            };

            _tipButton.StoreBinding.Bind(SettingsBinding.Child(c => c.TipButton));
            _eraserButton.StoreBinding.Bind(SettingsBinding.Child(c => c.EraserButton));
            _tipThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.TipActivationThreshold));
            _eraserThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.EraserActivationThreshold));
            _penButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettings>)c.PenButtons));
        }

        private readonly BindingDisplay _tipButton, _eraserButton;
        private readonly FloatSlider _tipThreshold, _eraserThreshold;
        private readonly BindingDisplayList _penButtons;
    }
}
