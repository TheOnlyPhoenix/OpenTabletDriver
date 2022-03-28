using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.UX.Controls.Generic.Reflection
{
    public sealed class TypeDropDown<T> : DropDown<TypeInfo> where T : class
    {
        private readonly IPluginManager _pluginManager;

        public TypeDropDown(IPluginManager pluginManager)
        {
            _pluginManager = pluginManager;

            ItemTextBinding = Binding.Property<TypeInfo, string>(t => t.GetFriendlyName() ?? t.GetPath());
            ItemKeyBinding = Binding.Property<TypeInfo, string>(t => t.GetPath());

            pluginManager.AssembliesChanged += HandleAssembliesChanged;
        }

        protected override IEnumerable<object> CreateDefaultDataStore()
        {
            var query = from type in _pluginManager.ExportedTypes
                where type.IsAssignableTo(typeof(T))
                orderby type.GetFriendlyName() ?? type.GetPath()
                select type;
            return query.ToList();
        }

        private void HandleAssembliesChanged(object? sender, EventArgs e) => DataStore = CreateDefaultDataStore();
    }
}
