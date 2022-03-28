using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSetting : NotifyPropertyChanged
    {
        public PluginSetting(string property, object value)
            : this()
        {
            Property = property;
            SetValue(value);
        }

        public PluginSetting(PropertyInfo property, object value)
            : this(property.Name, value)
        {
        }

        public PluginSetting(PropertyInfo property)
            : this(property, null)
        {
        }

        [JsonConstructor]
        private PluginSetting()
        {
        }

        private string _property;
        private JToken _value;

        [JsonProperty]
        public string Property
        {
            set => RaiseAndSetIfChanged(ref _property, value);
            get => _property;
        }

        [JsonProperty]
        public JToken Value
        {
            set => RaiseAndSetIfChanged(ref _value, value);
            get => _value;
        }

        [JsonIgnore]
        public bool HasValue => Value != null && Value.Type != JTokenType.Null;

        public void SetValue(object value)
        {
            Value = value == null ? null : JToken.FromObject(value);
        }

        public T GetValue<T>()
        {
            return Value == null ? default : Value.Type != JTokenType.Null ? Value.ToObject<T>() : default;
        }

        public object GetValue(Type asType)
        {
            return Value == null ? default : Value.Type != JTokenType.Null ? Value.ToObject(asType) : default;
        }

        public override string ToString()
        {
            return Property + ": " + Value;
        }
    }
}
