using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Windows.Bindings;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingDisplay : Panel
    {
        public BindingDisplay(IPluginFactory pluginFactory)
        {
            Content = new StackLayout
            {
                Spacing = 5,
                MinimumSize = new Size(300, 0),
                Orientation = Orientation.Horizontal,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = _mainButton = new Button()
                    },
                    new StackLayoutItem
                    {
                        Control = _advancedButton = new Button
                        {
                            Text = "...",
                            Width = 25
                        }
                    }
                }
            };

            _mainButton.TextBinding.Bind(StoreBinding.Convert<string>(s => pluginFactory.GetFriendlyName(s.Path) ?? s.Path));

            _mainButton.Click += async (sender, e) =>
            {
                var dialog = new BindingEditorDialog(Settings);
                Settings = await dialog.ShowModalAsync(this);
            };

            _advancedButton.Click += async (sender, e) =>
            {
                var dialog = new AdvancedBindingEditorDialog(Settings);
                Settings = await dialog.ShowModalAsync(this);
            };
        }

        private readonly Button _mainButton, _advancedButton;

        public event EventHandler<EventArgs> StoreChanged;

        private PluginSettings _settings;
        public PluginSettings Settings
        {
            set
            {
                _settings = value;
                StoreChanged?.Invoke(this, EventArgs.Empty);
            }
            get => _settings;
        }

        public BindableBinding<BindingDisplay, PluginSettings> StoreBinding
        {
            get
            {
                return new BindableBinding<BindingDisplay, PluginSettings>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.StoreChanged += h,
                    (c, h) => c.StoreChanged -= h
                );
            }
        }
    }
}
