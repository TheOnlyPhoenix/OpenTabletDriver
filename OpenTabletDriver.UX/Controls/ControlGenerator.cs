using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Attributes.UI;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Output.Generic;

namespace OpenTabletDriver.UX.Controls
{
    public class ControlGenerator : IControlGenerator
    {
        public ControlGenerator()
        {
            _genericControls = new Dictionary<Type, Func<PropertyInfo, SettingAttribute, DirectBinding<PluginSetting>, Control>>
            {
                { typeof(bool), GetCheckbox },
                { typeof(sbyte), GetNumericMaskedTextBox<sbyte> },
                { typeof(byte), GetNumericMaskedTextBox<byte> },
                { typeof(short), GetNumericMaskedTextBox<short> },
                { typeof(ushort), GetNumericMaskedTextBox<ushort> },
                { typeof(int), GetNumericMaskedTextBox<int> },
                { typeof(uint), GetNumericMaskedTextBox<uint> },
                { typeof(long), GetNumericMaskedTextBox<long> },
                { typeof(ulong), GetNumericMaskedTextBox<ulong> },
                { typeof(double), GetMaskedTextBox<DoubleNumberBox, double> },
                { typeof(DateTime), GetMaskedTextBox<DateTime> },
                { typeof(TimeSpan), GetMaskedTextBox<TimeSpan> },
                { typeof(Area), GetAreaEditor<AreaEditor<Area>, Area> },
                { typeof(AngledArea), GetAreaEditor<AngledAreaEditor, AngledArea> }
            };
        }

        private readonly IDictionary<Type, Func<PropertyInfo, SettingAttribute, DirectBinding<PluginSetting>, Control>> _genericControls;

        public Control Generate(TypeInfo type, DirectBinding<PluginSettings> settings)
        {
            if (settings.DataValue == null)
                return new Panel();

            var controls = new List<Control>();
            var areaControls = new Dictionary<PropertyInfo, AreaEditor<Area>>();
            var linkedSettingControls = new Dictionary<string, DirectBinding<bool>>();

            foreach (var property in type.GetProperties())
            {
                var attr = property.GetCustomAttribute<SettingAttribute>();
                if (property.CanRead && property.CanWrite && attr != null)
                {
                    var binding = settings.Child(c => c![property.Name]);
                    var control = GetControlForSetting(property, attr, binding);
                    controls.Add(control);

                    if (LinkedSettingAttribute.SupportedTypes.Any(t => t.IsAssignableFrom(property.PropertyType)))
                    {
                        var linkedAttr = property.PropertyType.GetCustomAttribute<LinkedSettingAttribute>();
                        if (linkedAttr != null && control is AreaEditor<Area> editor)
                            areaControls.Add(property, editor);
                    }
                    else if (property.GetCustomAttribute<LinkedSettingSourceAttribute>() != null)
                    {
                        linkedSettingControls.Add(property.Name, binding.Setting<bool>());
                    }
                }
            }

            foreach (var pair in areaControls)
            {
                pair.Deconstruct(out var property, out var sourceEditor);

                var linkedAttr = property.PropertyType.GetCustomAttribute<LinkedSettingAttribute>()!;
                if (linkedSettingControls.TryGetValue(linkedAttr.SettingMemberName, out var linkBinding))
                {
                    var targetPair = areaControls.First(c => c.Key.Name == linkedAttr.TargetMemberName);
                    areaControls.Remove(targetPair.Key);

                    targetPair.Deconstruct(out var targetProperty, out var targetEditor);

                    // TODO: Perform aspect ratio lock!

                    // bool handlingArLock = false;
                    //
                    // void AspectRatioLock(object? sender, EventArgs args)
                    // {
                    //     if (!handlingArLock)
                    //     {
                    //         // Avoids looping
                    //         handlingArLock = true;
                    //
                    //         var sourceWidth = sourceEditor.Area.Width;
                    //         var sourceHeight = sourceEditor.Area.Height;
                    //         var targetWidth = targetEditor.Area.Width;
                    //         var targetHeight = targetEditor.Area.Height;
                    //
                    //         if (sender == tabletWidth || sender == tabletAreaEditor)
                    //             tabletHeight.DataValue = displayHeight.DataValue / displayWidth.DataValue * tabletWidth.DataValue;
                    //         else if (sender == tabletHeight)
                    //             tabletWidth.DataValue = displayWidth.DataValue / displayHeight.DataValue * tabletHeight.DataValue;
                    //         else if ((sender == displayWidth) && prevDisplayWidth is float prevWidth)
                    //             tabletWidth.DataValue *= displayWidth.DataValue / prevWidth;
                    //         else if ((sender == displayHeight) && prevDisplayHeight is float prevHeight) tabletHeight.DataValue *= displayHeight.DataValue / prevHeight;
                    //
                    //         prevDisplayWidth = displayWidth.DataValue;
                    //         prevDisplayHeight = displayHeight.DataValue;
                    //
                    //         handlingArLock = false;
                    //     }
                    // }
                    //
                    // sourceEditor.AreaBinding.DataValueChanged += AspectRatioLock;
                }
            }

            var items = controls.Select(c => new StackLayoutItem(c)).ToArray();

            return new StackLayout(items)
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
        }

        public Control GetControlForSetting(PropertyInfo property, SettingAttribute attribute, DirectBinding<PluginSetting> binding)
        {
            var type = property.PropertyType;
            if (_genericControls.ContainsKey(type))
            {
                var factory = _genericControls[type];
                return factory.Invoke(property, attribute, binding);
            }

            throw new NotSupportedException("Settings of this type are not supported by OpenTabletDriver.");
        }

        private NumericMaskedTextBox<T> GetNumericMaskedTextBox<T>(PropertyInfo property, SettingAttribute attribute, DirectBinding<PluginSetting> binding)
        {
            return GetMaskedTextBox<NumericMaskedTextBox<T>, T>(property, attribute, binding);
        }

        private MaskedTextBox<T> GetMaskedTextBox<T>(PropertyInfo property, SettingAttribute attribute, DirectBinding<PluginSetting> binding)
        {
            return GetMaskedTextBox<MaskedTextBox<T>, T>(property, attribute, binding);
        }

        private TControl GetMaskedTextBox<TControl, T>(PropertyInfo property, SettingAttribute attribute, DirectBinding<PluginSetting> binding) where TControl : MaskedTextBox<T>, new()
        {
            var textBox = new TControl();
            textBox.ValueBinding.Bind(Convert<T>(binding));
            return textBox;
        }

        private Control GetCheckbox(PropertyInfo property, SettingAttribute attribute, DirectBinding<PluginSetting> binding)
        {
            var checkbox = new CheckBox();
            checkbox.CheckedBinding.Bind(Convert<bool?>(binding));
            return checkbox;
        }

        private TControl GetAreaEditor<TControl, T>(PropertyInfo property, SettingAttribute attribute, DirectBinding<PluginSetting> binding) where T : Area, new() where TControl : AreaEditor<T>, new()
        {
            var editor = new TControl();
            var area = Convert<T>(binding);
            editor.AreaBinding.Bind(area);

            return editor;
        }

        private static DirectBinding<T> Convert<T>(DirectBinding<PluginSetting> binding)
        {
            return binding.Convert(v => v.GetValue<T>(), b =>
            {
                binding.DataValue.SetValue(b);
                return binding.DataValue;
            });
        }
    }
}
