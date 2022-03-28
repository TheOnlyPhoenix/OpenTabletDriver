using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using JetBrains.Annotations;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Utilities;

namespace OpenTabletDriver.UX.Controls.Output.Generic
{
    [PublicAPI]
    public class AreaEditor<T> : Panel where T : Area, new()
    {
        public AreaEditor()
        {
            Content = new StackLayout
            {
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Control = new Panel
                        {
                            Padding = new Padding(5),
                            Content = Display = new AreaDisplay<T>()
                        }
                    },
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Control = SettingsPanel = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Control = _widthGroup = new UnitGroup
                                    {
                                        Text = "Width",
                                        Unit = Unit,
                                        ToolTip = $"Area width in {Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = _width = new FloatNumberBox()
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = _heightGroup = new UnitGroup
                                    {
                                        Text = "Height",
                                        ToolTip = $"Area height in {Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = _height = new FloatNumberBox()
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = _xGroup = new UnitGroup
                                    {
                                        Text = "X",
                                        ToolTip = $"Area center X offset in {Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = _x = new FloatNumberBox()
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Control = _yGroup = new UnitGroup
                                    {
                                        Text = "Y",
                                        Unit = Unit,
                                        ToolTip = $"Area center Y offset in {Unit}",
                                        Orientation = Orientation.Horizontal,
                                        Content = _y = new FloatNumberBox()
                                    }
                                }
                            }
                        }
                    }
                }
            };

            CreateMenu();

            _widthGroup.UnitBinding.Bind(UnitBinding);
            _heightGroup.UnitBinding.Bind(UnitBinding);
            _xGroup.UnitBinding.Bind(UnitBinding);
            _yGroup.UnitBinding.Bind(UnitBinding);

            var widthBinding = AreaBinding.Child((T s) => s.Width);
            var heightBinding = AreaBinding.Child((T s) => s.Height);
            var xBinding = AreaBinding.Child(s => s.Position.X);
            var yBinding = AreaBinding.Child(s => s.Position.Y);

            _width.ValueBinding.Bind(widthBinding);
            _height.ValueBinding.Bind(heightBinding);
            _x.ValueBinding.Bind(xBinding);
            _y.ValueBinding.Bind(yBinding);

            Display.AreaBinding.Bind(AreaBinding);
            Display.LockToUsableAreaBinding.Bind(LockToUsableAreaBinding);
            Display.UnitBinding.Bind(UnitBinding);
            Display.AreaBoundsBinding.Bind(AreaBoundsBinding);
            Display.InvalidForegroundErrorBinding.Bind(InvalidForegroundErrorBinding);
            Display.InvalidBackgroundErrorBinding.Bind(InvalidBackgroundErrorBinding);
        }

        private UnitGroup _widthGroup, _heightGroup, _xGroup, _yGroup;
        private MaskedTextBox<float> _width, _height, _x, _y;
        private bool _lockToUsableArea;

        protected StackLayout SettingsPanel;

        public AreaDisplay<T> Display { get; }

        protected virtual void CreateMenu()
        {
            var lockToUsableArea = new BooleanCommand
            {
                MenuText = "Lock to usable area"
            };

            ContextMenu = new ContextMenu
            {
                Items =
                {
                    new ButtonMenuItem
                    {
                        Text = "Align",
                        Items =
                        {
                            new ActionCommand
                            {
                                MenuText = "Left",
                                Action = () =>
                                {
                                    var x = Area.CenterOffset.X;
                                    Area.Position = new Vector2(x, Area.Position.Y);
                                }
                            },
                            new ActionCommand
                            {
                                MenuText = "Right",
                                Action = () =>
                                {
                                    var x = FullAreaBounds.Width - Area.CenterOffset.X;
                                    Area.Position = new Vector2(x, Area.Position.Y);
                                }
                            },
                            new ActionCommand
                            {
                                MenuText = "Top",
                                Action = () =>
                                {
                                    var y = Area.CenterOffset.Y;
                                    Area.Position = new Vector2(Area.Position.X, y);
                                }
                            },
                            new ActionCommand
                            {
                                MenuText = "Bottom",
                                Action = () =>
                                {
                                    var y = FullAreaBounds.Height - Area.CenterOffset.Y;
                                    Area.Position = new Vector2(Area.Position.X, y);
                                }
                            },
                            new ActionCommand
                            {
                                MenuText = "Center",
                                Action = () =>
                                {
                                    Area.Position = new Vector2(FullAreaBounds.Center.X, FullAreaBounds.Center.Y);
                                }
                            }
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "Resize",
                        Items =
                        {
                            new ActionCommand
                            {
                                MenuText = "Full area",
                                Action = () =>
                                {
                                    Area.Height = FullAreaBounds.Height;
                                    Area.Width = FullAreaBounds.Width;
                                    var x = FullAreaBounds.Center.X;
                                    var y = _fullAreaBounds.Center.Y;
                                    Area.Position = new Vector2(x, y);
                                }
                            },
                            new ActionCommand
                            {
                                MenuText = "Quarter area",
                                Action = () =>
                                {
                                    Area.Height = FullAreaBounds.Height / 2;
                                    Area.Width = FullAreaBounds.Width / 2;
                                }
                            }
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "Flip",
                        Items =
                        {
                            new ActionCommand
                            {
                                MenuText = "Horizontal",
                                Action = () =>
                                {
                                    var x = FullAreaBounds.Width - Area.Position.X;
                                    Area.Position = new Vector2(x, Area.Position.Y);
                                }
                            },
                            new ActionCommand
                            {
                                MenuText = "Vertical",
                                Action = () =>
                                {
                                    var y = FullAreaBounds.Height - Area.Position.Y;
                                    Area.Position = new Vector2(Area.Position.X, y);
                                }
                            }
                        }
                    },
                    lockToUsableArea
                }
            };

            lockToUsableArea.CheckedBinding.Cast<bool>().Bind(LockToUsableAreaBinding);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            switch (e.Buttons)
            {
                case MouseButtons.Alternate:
                {
                    ContextMenu.Show(this);
                    break;
                }
            }
        }

        private T? _area;
        private string _unit, _invalidForegroundError, _invalidBackgroundError;
        private IEnumerable<RectangleF> _areaBounds;
        private RectangleF _fullAreaBounds;

        public event EventHandler<EventArgs>? AreaChanged;
        public event EventHandler<EventArgs>? UnitChanged;
        public event EventHandler<EventArgs>? AreaBoundsChanged;
        public event EventHandler<EventArgs>? InvalidForegroundErrorChanged;
        public event EventHandler<EventArgs>? InvalidBackgroundErrorChanged;

        protected virtual void OnAreaChanged() => AreaChanged?.Invoke(this, EventArgs.Empty);
        protected virtual void OnUnitChanged() => UnitChanged?.Invoke(this, EventArgs.Empty);
        protected virtual void OnAreaBoundsChanged() => AreaBoundsChanged?.Invoke(this, EventArgs.Empty);
        protected virtual void OnInvalidForegroundErrorChanged() => InvalidForegroundErrorChanged?.Invoke(this, EventArgs.Empty);
        protected virtual void OnInvalidBackgroundErrorChanged() => InvalidBackgroundErrorChanged?.Invoke(this, EventArgs.Empty);

        public T? Area
        {
            set
            {
                _area = value;
                OnAreaChanged();
            }
            get => _area;
        }

        public string Unit
        {
            set
            {
                _unit = value;
                OnUnitChanged();
            }
            get => _unit;
        }

        public virtual IEnumerable<RectangleF> AreaBounds
        {
            set
            {
                _areaBounds = value;
                OnAreaBoundsChanged();
            }
            get => _areaBounds;
        }

        public RectangleF FullAreaBounds
        {
            get => FullAreaBoundsBinding.DataValue;
        }

        public string InvalidForegroundError
        {
            set
            {
                _invalidForegroundError = value;
                OnInvalidForegroundErrorChanged();
            }
            get => _invalidForegroundError;
        }

        public string InvalidBackgroundError
        {
            set
            {
                _invalidBackgroundError = value;
                OnInvalidBackgroundErrorChanged();
            }
            get => _invalidBackgroundError;
        }

        public BindableBinding<AreaEditor<T>, T?> AreaBinding
        {
            get
            {
                return new BindableBinding<AreaEditor<T>, T?>(
                    this,
                    c => c.Area,
                    (c, v) => c.Area = v,
                    (c, h) => c.AreaChanged += h,
                    (c, h) => c.AreaChanged -= h
                );
            }
        }

        public BindableBinding<AreaEditor<T>, string> UnitBinding
        {
            get
            {
                return new BindableBinding<AreaEditor<T>, string>(
                    this,
                    c => c.Unit,
                    (c, v) => c.Unit = v,
                    (c, h) => c.UnitChanged += h,
                    (c, h) => c.UnitChanged -= h
                );
            }
        }

        public BindableBinding<AreaEditor<T>, IEnumerable<RectangleF>> AreaBoundsBinding
        {
            get
            {
                return new BindableBinding<AreaEditor<T>, IEnumerable<RectangleF>>(
                    this,
                    c => c.AreaBounds,
                    (c, v) => c.AreaBounds = v,
                    (c, h) => c.AreaBoundsChanged += h,
                    (c, h) => c.AreaBoundsChanged -= h
                );
            }
        }

        public BindableBinding<AreaEditor<T>, RectangleF> FullAreaBoundsBinding
        {
            get
            {
                return AreaBoundsBinding.Convert(b =>
                {
                    return new RectangleF
                    {
                        Left = b.Min(r => r.Left),
                        Top = b.Min(r => r.Top),
                        Right = b.Max(r => r.Right),
                        Bottom = b.Max(r => r.Bottom),
                    };
                });
            }
        }

        public BindableBinding<AreaEditor<T>, string> InvalidForegroundErrorBinding
        {
            get
            {
                return new BindableBinding<AreaEditor<T>, string>(
                    this,
                    c => c.InvalidForegroundError,
                    (c, v) => c.InvalidForegroundError = v,
                    (c, h) => c.InvalidForegroundErrorChanged += h,
                    (c, h) => c.InvalidForegroundErrorChanged -= h
                );
            }
        }

        public BindableBinding<AreaEditor<T>, string> InvalidBackgroundErrorBinding
        {
            get
            {
                return new BindableBinding<AreaEditor<T>, string>(
                    this,
                    c => c.InvalidBackgroundError,
                    (c, v) => c.InvalidBackgroundError = v,
                    (c, h) => c.InvalidBackgroundErrorChanged += h,
                    (c, h) => c.InvalidBackgroundErrorChanged -= h
                );
            }
        }

        public BindableBinding<AreaEditor<T>, bool> LockToUsableAreaBinding
        {
            get
            {
                return new BindableBinding<AreaEditor<T>, bool>(
                    this,
                    c => c.LockToUsableArea,
                    (c, v) => c.LockToUsableArea = v,
                    (c, h) => c.LockToUsableAreaChanged += h,
                    (c, h) => c.LockToUsableAreaChanged -= h
                );
            }
        }

        public bool LockToUsableArea
        {
            set
            {
                _lockToUsableArea = value;
                OnLockToUsableAreaChanged();
            }
            get => _lockToUsableArea;
        }

        protected virtual void OnLockToUsableAreaChanged()
        {
            LockToUsableAreaChanged?.Invoke(this, EventArgs.Empty);
            if (LockToUsableArea)
                OnAreaChanged();
        }

        public event EventHandler<EventArgs>? LockToUsableAreaChanged;
    }
}
