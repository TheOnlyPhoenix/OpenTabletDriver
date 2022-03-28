using System;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Output.Generic
{
    public class UnitGroup : Group
    {
        public UnitGroup()
        {
            _unitLabel.TextBinding.Bind(UnitBinding);
        }

        private string? _unit;
        public string? Unit
        {
            set
            {
                _unit = value;
                OnUnitChanged();
            }
            get => _unit;
        }

        public event EventHandler<EventArgs>? UnitChanged;

        protected virtual void OnUnitChanged() => UnitChanged?.Invoke(this, EventArgs.Empty);

        public BindableBinding<UnitGroup, string?> UnitBinding
        {
            get
            {
                return new BindableBinding<UnitGroup, string?>(
                    this,
                    c => c.Unit,
                    (c, v) => c.Unit = v,
                    (c, h) => c.UnitChanged += h,
                    (c, h) => c.UnitChanged -= h
                );
            }
        }

        private readonly Label _unitLabel = new Label();

        private Control? _content;
        public new Control? Content
        {
            set
            {
                _content = value;
                base.Content = new StackLayout
                {
                    Spacing = 5,
                    Orientation = Orientation.Horizontal,
                    Items =
                    {
                        new StackLayoutItem(Content, true),
                        new StackLayoutItem
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            Control = _unitLabel
                        }
                    }
                };
            }
            get => _content;
        }
    }
}
