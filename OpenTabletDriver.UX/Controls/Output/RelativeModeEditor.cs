using System;
using System.Numerics;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Output.Generic;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class RelativeModeEditor : Panel
    {
        public RelativeModeEditor()
        {
            var xSens = new FloatNumberBox();
            var ySens = new FloatNumberBox();
            var rotation = new FloatNumberBox();
            var resetDelay = new FloatNumberBox();

            Content = new Group
            {
                Text = "Relative",
                Content = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    Spacing = 5,
                    Items =
                    {
                        new StackLayoutItem(null, true),
                        new UnitGroup
                        {
                            Text = "X Sensitivity",
                            Orientation = Orientation.Horizontal,
                            Unit = "px/mm",
                            Content = xSens
                        },
                        new UnitGroup
                        {
                            Text = "Y Sensitivity",
                            Orientation = Orientation.Horizontal,
                            Unit = "px/mm",
                            Content = ySens
                        },
                        new UnitGroup
                        {
                            Text = "Rotation",
                            Orientation = Orientation.Horizontal,
                            Unit = "°",
                            Content = rotation
                        },
                        new UnitGroup
                        {
                            Text = "Reset Time",
                            Orientation = Orientation.Horizontal,
                            Unit = "ms",
                            Content = resetDelay
                        },
                        new StackLayoutItem(null, true)
                    }
                }
            };

            var sensitivityBinding = SettingsBinding.BindSetting<Vector2>(nameof(RelativeOutputMode.Sensitivity));
            var xSensBinding = sensitivityBinding.Convert(
                v => v.X,
                x => new Vector2(x, sensitivityBinding.DataValue.Y)
            );
            var ySensBinding = sensitivityBinding.Convert(
                v => v.Y,
                y => new Vector2(sensitivityBinding.DataValue.X, y)
            );

            var rotationBinding = SettingsBinding.BindSetting<float>(nameof(RelativeOutputMode.Rotation));
            var resetDelayBinding = SettingsBinding.BindSetting<TimeSpan>(nameof(RelativeOutputMode.ResetDelay));

            xSens.ValueBinding.Bind(xSensBinding);
            ySens.ValueBinding.Bind(ySensBinding);
            rotation.ValueBinding.Bind(rotationBinding);
            resetDelay.ValueBinding.Convert(
                c => TimeSpan.FromMilliseconds(c),
                v => (float)v.TotalMilliseconds
            ).Bind(resetDelayBinding);
        }

        private PluginSettings? _settings;
        public PluginSettings? Settings
        {
            set
            {
                _settings = value;
                OnSettingsChanged();
            }
            get => _settings;
        }

        public event EventHandler<EventArgs> SettingsChanged;

        protected virtual void OnSettingsChanged() => SettingsChanged?.Invoke(this, EventArgs.Empty);

        public BindableBinding<RelativeModeEditor, PluginSettings?> SettingsBinding
        {
            get
            {
                return new BindableBinding<RelativeModeEditor, PluginSettings?>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }
    }
}
