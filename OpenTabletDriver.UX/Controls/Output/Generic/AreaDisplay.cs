using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using JetBrains.Annotations;
using OpenTabletDriver.Interop;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Output.Generic
{
    [PublicAPI]
    public class AreaDisplay<T> : ScheduledDrawable where T : Area, new()
    {
        private T? _area;
        private bool _lockToUsableArea;
        private string? _unit, _invalidForegroundError, _invalidBackgroundError;
        private IEnumerable<RectangleF>? _areaBounds;
        private RectangleF _fullAreaBounds;

        private readonly TextDrawer _textDrawer = new TextDrawer();

        public event EventHandler<EventArgs>? AreaChanged;
        public event EventHandler<EventArgs>? LockToUsableAreaChanged;
        public event EventHandler<EventArgs>? UnitChanged;
        public event EventHandler<EventArgs>? AreaBoundsChanged;
        public event EventHandler<EventArgs>? InvalidForegroundErrorChanged;
        public event EventHandler<EventArgs>? InvalidBackgroundErrorChanged;

        protected virtual void OnAreaChanged() => AreaChanged?.Invoke(this, EventArgs.Empty);
        protected virtual void OnLockToUsableAreaChanged() => LockToUsableAreaChanged?.Invoke(this, EventArgs.Empty);
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

        public bool LockToUsableArea
        {
            set
            {
                _lockToUsableArea = value;
                OnLockToUsableAreaChanged();
            }
            get => _lockToUsableArea;
        }

        public string? Unit
        {
            set
            {
                _unit = value;
                OnUnitChanged();
            }
            get => _unit;
        }

        public virtual IEnumerable<RectangleF>? AreaBounds
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

        public string? InvalidForegroundError
        {
            set
            {
                _invalidForegroundError = value;
                OnInvalidForegroundErrorChanged();
            }
            get => _invalidForegroundError;
        }

        public string? InvalidBackgroundError
        {
            set
            {
                _invalidBackgroundError = value;
                OnInvalidBackgroundErrorChanged();
            }
            get => _invalidBackgroundError;
        }

        public BindableBinding<AreaDisplay<T>, T?> AreaBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay<T>, T?>(
                    this,
                    c => c.Area,
                    (c, v) => c.Area = v,
                    (c, h) => c.AreaChanged += h,
                    (c, h) => c.AreaChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay<T>, bool> LockToUsableAreaBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay<T>, bool>(
                    this,
                    c => c.LockToUsableArea,
                    (c, v) => c.LockToUsableArea = v,
                    (c, h) => c.LockToUsableAreaChanged += h,
                    (c, h) => c.LockToUsableAreaChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay<T>, string> UnitBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay<T>, string>(
                    this,
                    c => c.Unit,
                    (c, v) => c.Unit = v,
                    (c, h) => c.UnitChanged += h,
                    (c, h) => c.UnitChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay<T>, IEnumerable<RectangleF>> AreaBoundsBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay<T>, IEnumerable<RectangleF>>(
                    this,
                    c => c.AreaBounds,
                    (c, v) => c.AreaBounds = v,
                    (c, h) => c.AreaBoundsChanged += h,
                    (c, h) => c.AreaBoundsChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay<T>, RectangleF> FullAreaBoundsBinding
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
                        Bottom = b.Max(r => r.Bottom)
                    };
                });
            }
        }

        public BindableBinding<AreaDisplay<T>, string?> InvalidForegroundErrorBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay<T>, string?>(
                    this,
                    c => c.InvalidForegroundError,
                    (c, v) => c.InvalidForegroundError = v,
                    (c, h) => c.InvalidForegroundErrorChanged += h,
                    (c, h) => c.InvalidForegroundErrorChanged -= h
                );
            }
        }

        public BindableBinding<AreaDisplay<T>, string?> InvalidBackgroundErrorBinding
        {
            get
            {
                return new BindableBinding<AreaDisplay<T>, string?>(
                    this,
                    c => c.InvalidBackgroundError,
                    (c, v) => c.InvalidBackgroundError = v,
                    (c, h) => c.InvalidBackgroundErrorChanged += h,
                    (c, h) => c.InvalidBackgroundErrorChanged -= h
                );
            }
        }

        private readonly Font _font = SystemFonts.User(8);
        private readonly Brush _textBrush = new SolidBrush(SystemColors.ControlText);

        private readonly Color _accentColor = SystemColors.Highlight;
        private readonly Color _areaBoundsFillColor = SystemColors.ControlBackground;
        private readonly Color _areaBoundsBorderColor = SystemInterop.CurrentPlatform switch
        {
            SystemPlatform.Windows => new Color(64, 64, 64),
            _ => SystemColors.Control
        };

        private bool _mouseDragging;
        private PointF? _mouseOffset;
        private PointF? _viewModelOffset;

        private RectangleF ForegroundRect => Area == null ? RectangleF.Empty : RectangleF.FromCenter(
            new PointF(Area.Position.X, Area.Position.Y),
            new SizeF(Area.Width, Area.Height)
        );

        public float PixelScale => CalculateScale(FullAreaBounds);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            switch (e.Buttons)
            {
                case MouseButtons.Primary:
                    _mouseDragging = true;
                    break;
                default:
                    _mouseDragging = false;
                    break;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (Area == null)
                return;

            switch (e.Buttons)
            {
                case MouseButtons.Primary:
                {
                    _mouseDragging = false;
                    break;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (Area == null)
                return;

            if (_mouseDragging)
            {
                if (_mouseOffset != null && _viewModelOffset != null)
                {
                    var delta = e.Location - _mouseOffset.Value;
                    var x = _viewModelOffset.Value.X + (delta.X / PixelScale);
                    var y = _viewModelOffset.Value.Y + (delta.Y / PixelScale);

                    Area.Position = new Vector2(x, y);
                    OnAreaChanged();
                }
                else
                {
                    _mouseOffset = e.Location;
                    _viewModelOffset = new PointF(Area.Position.X, Area.Position.Y);
                }
            }
            else if (!_mouseDragging && _mouseOffset != null)
            {
                _mouseOffset = null;
                _viewModelOffset = null;
            }
        }

        protected override void OnNextFrame(PaintEventArgs e)
        {
            var graphics = e.Graphics;

            switch (IsValid(ForegroundRect), IsValid(FullAreaBounds))
            {
                case (true, true):
                {
                    using (graphics.SaveTransformState())
                    {
                        var scale = CalculateScale(FullAreaBounds);

                        var clientCenter = new PointF(ClientSize.Width, ClientSize.Height) / 2;
                        var backgroundCenter = new PointF(FullAreaBounds.Width, FullAreaBounds.Height) / 2 * scale;
                        var offset = clientCenter - backgroundCenter;

                        graphics.TranslateTransform(offset);

                        DrawBackground(graphics, scale);
                        DrawForeground(graphics, scale);
                    }
                    break;
                }
                case (_, false):
                {
                    DrawText(graphics, InvalidBackgroundError);
                    break;
                }
                case (false, _):
                {
                    DrawText(graphics, InvalidForegroundError);
                    break;
                }
            }
        }

        private void DrawBackground(Graphics graphics, float scale)
        {
            using (graphics.SaveTransformState())
            {
                graphics.TranslateTransform(-FullAreaBounds.TopLeft * scale);
                foreach (var rect in AreaBounds)
                {
                    var scaledRect = rect * scale;
                    graphics.FillRectangle(_areaBoundsFillColor, scaledRect);
                    graphics.DrawRectangle(_areaBoundsBorderColor, scaledRect);
                }
            }
        }

        private void DrawForeground(Graphics graphics, float scale)
        {
            using (graphics.SaveTransformState())
            {
                var area = ForegroundRect * scale;

                if (Area is AngledArea angledArea)
                {
                    graphics.TranslateTransform(area.Center);
                    graphics.RotateTransform(angledArea.Rotation);
                    graphics.TranslateTransform(-area.Center);
                }

                graphics.FillRectangle(_accentColor, area);

                var originEllipse = new RectangleF(0, 0, 1, 1);
                originEllipse.Offset(area.Center - (originEllipse.Size / 2));
                graphics.DrawEllipse(SystemColors.ControlText, originEllipse);

                DrawRatioText(graphics, area);
                DrawWidthText(graphics, area);
                DrawHeightText(graphics, area);
            }
        }

        private void DrawRatioText(Graphics graphics, RectangleF area)
        {
            if (Area == null)
                return;

            string ratio = Math.Round(Area.Width / Area.Height, 4).ToString();
            SizeF ratioMeasure = graphics.MeasureString(_font, ratio);
            var offsetY = area.Center.Y + (ratioMeasure.Height / 2);
            if (offsetY + ratioMeasure.Height > area.Y + area.Height)
                offsetY = area.Y + area.Height;

            var ratioPos = new PointF(
                area.Center.X - (ratioMeasure.Width / 2),
                offsetY
            );
            _textDrawer.DrawText(graphics, _font, _textBrush, ratioPos, ratio);
        }

        private void DrawWidthText(Graphics graphics, RectangleF area)
        {
            if (Area == null)
                return;

            var minDist = area.Center.Y - 40;
            string widthText = $"{MathF.Round(Area.Width, 3)}{Unit}";
            var widthTextSize = graphics.MeasureString(_font, widthText);
            var widthTextPos = new PointF(
                area.MiddleTop.X - (widthTextSize.Width / 2),
                Math.Min(area.MiddleTop.Y, minDist)
            );
            _textDrawer.DrawText(graphics, _font, _textBrush, widthTextPos, widthText);
        }

        private void DrawHeightText(Graphics graphics, RectangleF area)
        {
            if (Area == null)
                return;

            using (graphics.SaveTransformState())
            {
                var minDist = area.Center.X - 40;
                string heightText = $"{MathF.Round(Area.Height, 3)}{Unit}";
                var heightSize = graphics.MeasureString(_font, heightText) / 2;
                var heightPos = new PointF(
                    -area.MiddleLeft.Y - heightSize.Width,
                    Math.Min(area.MiddleLeft.X, minDist)
                );
                graphics.RotateTransform(-90);
                _textDrawer.DrawText(graphics, _font, _textBrush, heightPos, heightText);
            }
        }

        private void DrawText(Graphics graphics, string errorText)
        {
            var errSize = graphics.MeasureString(_font, errorText);
            var errorOffset = new PointF(errSize.Width, errSize.Height) / 2;
            var clientOffset = new PointF(ClientSize.Width, ClientSize.Height) / 2;
            var offset = clientOffset - errorOffset;

            _textDrawer.DrawText(graphics, _font, _textBrush, offset, errorText);
        }

        private float CalculateScale(RectangleF rect)
        {
            float scaleX = (ClientSize.Width - 2) / rect.Width;
            float scaleY = (ClientSize.Height - 2) / rect.Height;
            return scaleX > scaleY ? scaleY : scaleX;
        }

        private static bool IsValid(RectangleF rect)
        {
            return rect.Width > 0 && rect.Height > 0;
        }
    }
}
