using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using JetBrains.Annotations;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Output.Generic;
using OpenTabletDriver.UX.Controls.Utilities;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class AbsoluteModeEditor : Panel
    {
        public AbsoluteModeEditor(IControlBuilder controlBuilder)
        {
            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Group
                        {
                            Text = "Display",
                            Content = _displayAreaEditor = controlBuilder.Build<DisplayAreaEditor>()
                        }
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Group
                        {
                            Text = "Tablet",
                            Content = _tabletAreaEditor = controlBuilder.Build<TabletAreaEditor>()
                        }
                    }
                }
            };

            var outputBinding = SettingsBinding.BindSetting<Area>(nameof(AbsoluteOutputMode.Output));
            var inputBinding = SettingsBinding.BindSetting<AngledArea>(nameof(AbsoluteOutputMode.Input));
            var usableAreaBinding = SettingsBinding.BindSetting<bool>(nameof(AbsoluteOutputMode.LockToBounds));
            var lockAspectRatioBinding = SettingsBinding.BindSetting<bool>(nameof(AbsoluteOutputMode.LockAspectRatio));
            var areaClippingBinding = SettingsBinding.BindSetting<bool>(nameof(AbsoluteOutputMode.AreaClipping));
            var areaLimitingBinding = SettingsBinding.BindSetting<bool>(nameof(AbsoluteOutputMode.AreaLimiting));

            _displayAreaEditor.AreaBinding.Bind(outputBinding);
            _tabletAreaEditor.AreaBinding.Bind(inputBinding);
            _tabletAreaEditor.LockToUsableAreaBinding.Bind(usableAreaBinding);

            _tabletAreaEditor.LockAspectRatioBinding.Bind(lockAspectRatioBinding);
            _tabletAreaEditor.AreaClippingBinding.Bind(areaClippingBinding);
            _tabletAreaEditor.IgnoreOutsideAreaBinding.Bind(areaLimitingBinding);

            _displayWidth = outputBinding.Child(c => c!.Width);
            _displayHeight = outputBinding.Child(c => c!.Height);
            _tabletWidth = inputBinding.Child(c => c!.Width);
            _tabletHeight = inputBinding.Child(c => c!.Height);

            _tabletWidth.DataValueChanged += HandleTabletAreaConstraint;
            _tabletHeight.DataValueChanged += HandleTabletAreaConstraint;
            _displayWidth.DataValueChanged += HandleDisplayAreaConstraint;
            _displayHeight.DataValueChanged += HandleDisplayAreaConstraint;

            _tabletAreaEditor.LockAspectRatioChanged += HookAspectRatioLock;
            HookAspectRatioLock(_tabletAreaEditor, EventArgs.Empty);
        }

        private readonly DisplayAreaEditor _displayAreaEditor;
        private readonly TabletAreaEditor _tabletAreaEditor;

        private bool _handlingArLock;
        private bool _handlingForcedArConstraint;
        private bool _handlingSettingsChanging;
        private float? _prevDisplayWidth;
        private float? _prevDisplayHeight;
        private DirectBinding<float> _displayWidth;
        private DirectBinding<float> _displayHeight;
        private DirectBinding<float> _tabletWidth;
        private DirectBinding<float> _tabletHeight;

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

        public event EventHandler<EventArgs>? SettingsChanged;

        protected virtual void OnSettingsChanged()
        {
            _handlingSettingsChanging = true;
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            _handlingSettingsChanging = false;
        }

        public BindableBinding<AbsoluteModeEditor, PluginSettings?> SettingsBinding
        {
            get
            {
                return new BindableBinding<AbsoluteModeEditor, PluginSettings?>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }

        public void SetOutputArea(IVirtualScreen virtualScreen)
        {
            var bgs = from disp in virtualScreen.Displays
                where disp is not IVirtualScreen
                select new RectangleF(disp.Position.X, disp.Position.Y, disp.Width, disp.Height);
            _displayAreaEditor.AreaBounds = bgs;
        }

        public void SetInputArea(TabletSpecifications specifications)
        {
            var digitizer = specifications.Digitizer;
            _tabletAreaEditor.AreaBounds = new[]
            {
                new RectangleF(0, 0, digitizer.Width, digitizer.Height)
            };
        }

        private void HookAspectRatioLock(object? sender, EventArgs args)
        {
            if (Settings?[nameof(AbsoluteOutputMode.LockAspectRatio)].GetValue<bool>() ?? false)
            {
                lock (this)
                {
                    HandleAspectRatioLock(_tabletAreaEditor, EventArgs.Empty);

                    _displayWidth.DataValueChanged += HandleAspectRatioLock;
                    _displayHeight.DataValueChanged += HandleAspectRatioLock;
                    _tabletWidth.DataValueChanged += HandleAspectRatioLock;
                    _tabletHeight.DataValueChanged += HandleAspectRatioLock;
                }
            }
            else
            {
                lock (this)
                {
                    _displayWidth.DataValueChanged -= HandleAspectRatioLock;
                    _displayHeight.DataValueChanged -= HandleAspectRatioLock;
                    _tabletWidth.DataValueChanged -= HandleAspectRatioLock;
                    _tabletHeight.DataValueChanged -= HandleAspectRatioLock;
                }
            }
        }

        // TODO: determine usage of this handler, appears unused.
        // private void HookAreaConstraint(object sender, EventArgs args)
        // {
        //     var areaEditor = (AreaEditor<Area>)sender;
        //     if (areaEditor.LockToUsableArea)
        //     {
        //         lock (this)
        //         {
        //             if (sender == _tabletAreaEditor)
        //             {
        //                 _tabletWidth.DataValueChanged += HandleTabletAreaConstraint;
        //                 _tabletHeight.DataValueChanged += HandleTabletAreaConstraint;
        //             }
        //             else if (sender == _displayAreaEditor)
        //             {
        //                 _displayWidth.DataValueChanged += HandleDisplayAreaConstraint;
        //                 _displayHeight.DataValueChanged += HandleDisplayAreaConstraint;
        //             }
        //         }
        //     }
        //     else
        //     {
        //         lock (this)
        //         {
        //             if (sender == _tabletAreaEditor)
        //             {
        //                 _tabletWidth.DataValueChanged -= HandleTabletAreaConstraint;
        //                 _tabletHeight.DataValueChanged -= HandleTabletAreaConstraint;
        //             }
        //             else if (sender == _displayAreaEditor)
        //             {
        //                 _displayWidth.DataValueChanged -= HandleDisplayAreaConstraint;
        //                 _displayHeight.DataValueChanged -= HandleDisplayAreaConstraint;
        //             }
        //         }
        //     }
        // }

        private void HandleAspectRatioLock(object? sender, EventArgs e)
        {
            if (!_handlingArLock && !_handlingSettingsChanging)
            {
                // Avoids looping
                _handlingArLock = true;

                if (sender == _tabletWidth || sender == _tabletAreaEditor)
                    _tabletHeight.DataValue = _displayHeight.DataValue / _displayWidth.DataValue * _tabletWidth.DataValue;
                else if (sender == _tabletHeight)
                    _tabletWidth.DataValue = _displayWidth.DataValue / _displayHeight.DataValue * _tabletHeight.DataValue;
                else if ((sender == _displayWidth) && _prevDisplayWidth is float prevWidth)
                    _tabletWidth.DataValue *= _displayWidth.DataValue / prevWidth;
                else if ((sender == _displayHeight) && _prevDisplayHeight is float prevHeight)
                    _tabletHeight.DataValue *= _displayHeight.DataValue / prevHeight;

                _prevDisplayWidth = _displayWidth.DataValue;
                _prevDisplayHeight = _displayHeight.DataValue;

                _handlingArLock = false;
            }
        }

        private void HandleTabletAreaConstraint(object? sender, EventArgs args)
        {
            ForceAreaConstraint(_tabletAreaEditor.Display, args);
        }

        private void HandleDisplayAreaConstraint(object? sender, EventArgs args)
        {
            ForceAreaConstraint(_displayAreaEditor.Display, args);
        }

        private void ForceAreaConstraint(object? sender, EventArgs args)
        {
            if (sender is not AreaDisplay<AngledArea> display)
                return;

            if (!_handlingForcedArConstraint && !_handlingSettingsChanging && display.LockToUsableArea && display.Area != null)
            {
                _handlingForcedArConstraint = true;
                var fullBounds = display.FullAreaBounds;

                if (fullBounds.Width != 0 && fullBounds.Height != 0)
                {
                    if (display.Area.Width > fullBounds.Width)
                        display.Area.Width = fullBounds.Width;
                    if (display.Area.Height > fullBounds.Height)
                        display.Area.Height = fullBounds.Height;

                    var correction = GetOutOfBoundsAmount(display, display.Area.Position);
                    display.Area.Position -= correction;
                }

                _handlingForcedArConstraint = false;
            }
        }

        // TODO: clean up likely duplicated code
        private static Vector2 GetOutOfBoundsAmount(AreaDisplay<AngledArea> display, Vector2 position)
        {
            var bounds = display.FullAreaBounds;
            bounds.X = 0;
            bounds.Y = 0;

            var area = display.Area!;
            var corners = area.Corners;

            var min = Vector2.Min(corners[0], Vector2.Min(corners[1], Vector2.Min(corners[2], corners[3])));
            var max = Vector2.Max(corners[0], Vector2.Max(corners[1], Vector2.Max(corners[2], corners[3])));
            var pseudoArea = new RectangleF(new PointF(min.X, min.Y), new PointF(max.X, max.Y));

            pseudoArea.Center += new PointF(position.X, position.Y);

            return new Vector2
            {
                X = Math.Max(pseudoArea.Right - bounds.Right - 1, 0) + Math.Min(pseudoArea.Left - bounds.Left, 0),
                Y = Math.Max(pseudoArea.Bottom - bounds.Bottom - 1, 0) + Math.Min(pseudoArea.Top - bounds.Top, 0)
            };
        }

        private sealed class DisplayAreaEditor : AreaEditor<Area>
        {
            private readonly IVirtualScreen _virtualScreen;

            public DisplayAreaEditor(IVirtualScreen virtualScreen)
            {
                _virtualScreen = virtualScreen;

                Unit = "px";
                ToolTip = "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";
            }

            protected override void CreateMenu()
            {
                base.CreateMenu();

                var subMenu = ContextMenu.Items.GetSubmenu("Set to display");
                foreach (var display in _virtualScreen.Displays)
                {
                    subMenu.Items.Add(
                        new ActionCommand
                        {
                            MenuText = display.ToString(),
                            Action = () =>
                            {
                                Area.Width = display.Width;
                                Area.Height = display.Height;
                                if (display is IVirtualScreen virtualScreen)
                                {
                                    var x = virtualScreen.Width / 2;
                                    var y = virtualScreen.Height / 2;
                                    Area.Position = new Vector2(x, y);
                                }
                                else
                                {
                                    var x = display.Position.X + _virtualScreen.Position.X + display.Width / 2;
                                    var y = display.Position.Y + _virtualScreen.Position.Y + display.Height / 2;
                                    Area.Position = new Vector2(x, y);
                                }
                            }
                        }
                    );
                }
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class TabletAreaEditor : AngledAreaEditor
        {
            public TabletAreaEditor()
            {
                Unit = "mm";
                InvalidBackgroundError = "No tablet detected.";
                ToolTip = "You can right click the area editor to enable aspect ratio locking, adjust alignment, or resize the area.";
            }

            private bool _lockAspectRatio, _areaClipping, _ignoreOutsideArea;

            public event EventHandler<EventArgs>? LockAspectRatioChanged;
            public event EventHandler<EventArgs>? AreaClippingChanged;
            public event EventHandler<EventArgs>? IgnoreOutsideAreaChanged;

            private void OnLockAspectRatioChanged() => LockAspectRatioChanged?.Invoke(this, EventArgs.Empty);
            private void OnAreaClippingChanged() => AreaClippingChanged?.Invoke(this, EventArgs.Empty);
            private void OnIgnoreOutsideAreaChanged() => IgnoreOutsideAreaChanged?.Invoke(this, EventArgs.Empty);

            public bool LockAspectRatio
            {
                set
                {
                    _lockAspectRatio = value;
                    OnLockAspectRatioChanged();
                }
                get => _lockAspectRatio;
            }

            public bool AreaClipping
            {
                set
                {
                    _areaClipping = value;
                    OnAreaClippingChanged();
                }
                get => _areaClipping;
            }

            public bool IgnoreOutsideArea
            {
                set
                {
                    _ignoreOutsideArea = value;
                    OnIgnoreOutsideAreaChanged();
                }
                get => _ignoreOutsideArea;
            }

            public BindableBinding<TabletAreaEditor, bool> LockAspectRatioBinding
            {
                get
                {
                    return new BindableBinding<TabletAreaEditor, bool>(
                        this,
                        c => c.LockAspectRatio,
                        (c, v) => c.LockAspectRatio = v,
                        (c, h) => c.LockAspectRatioChanged += h,
                        (c, h) => c.LockAspectRatioChanged -= h
                    );
                }
            }

            public BindableBinding<TabletAreaEditor, bool> AreaClippingBinding
            {
                get
                {
                    return new BindableBinding<TabletAreaEditor, bool>(
                        this,
                        c => c.AreaClipping,
                        (c, v) => c.AreaClipping = v,
                        (c, h) => c.AreaClippingChanged += h,
                        (c, h) => c.AreaClippingChanged -= h
                    );
                }
            }

            public BindableBinding<TabletAreaEditor, bool> IgnoreOutsideAreaBinding
            {
                get
                {
                    return new BindableBinding<TabletAreaEditor, bool>(
                        this,
                        c => c.IgnoreOutsideArea,
                        (c, v) => c.IgnoreOutsideArea = v,
                        (c, h) => c.IgnoreOutsideAreaChanged += h,
                        (c, h) => c.IgnoreOutsideAreaChanged -= h
                    );
                }
            }

            protected override void CreateMenu()
            {
                base.CreateMenu();

                ContextMenu.Items.AddSeparator();

                var lockArCmd = new BooleanCommand
                {
                    MenuText = "Lock aspect ratio"
                };

                var areaClippingCmd = new BooleanCommand
                {
                    MenuText = "Clamp input outside area"
                };

                var ignoreOutsideAreaCmd = new BooleanCommand
                {
                    MenuText = "Ignore input outside area"
                };

                ContextMenu.Items.AddRange(
                    new Command[]
                    {
                        lockArCmd,
                        areaClippingCmd,
                        ignoreOutsideAreaCmd
                    }
                );

                ContextMenu.Items.AddSeparator();

                ContextMenu.Items.Add(
                    new ActionCommand
                    {
                        MenuText = "Convert area...",
                        Action = () => Application.Instance.AsyncInvoke(ConvertAreaDialog)
                    }
                );

                lockArCmd.CheckedBinding.Cast<bool>().Bind(LockAspectRatioBinding);
                areaClippingCmd.CheckedBinding.Cast<bool>().Bind(AreaClippingBinding);
                ignoreOutsideAreaCmd.CheckedBinding.Cast<bool>().Bind(IgnoreOutsideAreaBinding);
            }

            private async Task ConvertAreaDialog()
            {
                var converter = new AreaConverterDialog
                {
                    DataContext = Area
                };
                await converter.ShowModalAsync(ParentWindow);
            }
        }
    }
}
