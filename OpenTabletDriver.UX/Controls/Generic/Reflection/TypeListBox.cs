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
    public sealed class TypeListBox<T> : ListBox<TypeInfo> where T : class
    {
        private readonly IPluginFactory _pluginFactory;

        public TypeListBox(IPluginFactory pluginFactory, IPluginManager pluginManager)
        {
            _pluginFactory = pluginFactory;

            ItemTextBinding = Binding.Property<TypeInfo, string>(t => t.GetFriendlyName() ?? t.GetPath());
            ItemKeyBinding = Binding.Property<TypeInfo, string>(t => t.GetPath());

            // Manual update of the DataStore seems to be required, however it isn't on DropDown. Bug?
            DataStore = CreateDefaultDataStore();

            pluginManager.AssembliesChanged += HandleAssembliesChanged;
        }

        protected override IEnumerable<object> CreateDefaultDataStore()
        {
            var query = from type in _pluginFactory.GetMatchingTypes(typeof(T))
                orderby type.GetFriendlyName()
                select type;
            return query.ToList();
        }

        private void HandleAssembliesChanged(object? sender, EventArgs e) => DataStore = CreateDefaultDataStore();
    }
}
