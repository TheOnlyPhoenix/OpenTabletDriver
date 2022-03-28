using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic.Reflection;

namespace OpenTabletDriver.UX.Controls
{
    public class PluginSettingStoreCollectionEditor<TSource> : Panel where TSource : class
    {
        private readonly IPluginManager _pluginManager;

        public PluginSettingStoreCollectionEditor(IPluginManager pluginManager, IControlBuilder controlBuilder)
        {
            _pluginManager = pluginManager;

            _settingStoreEditor = controlBuilder.Build<ToggleablePluginSettingStoreEditor>();
            _settingStoreEditor.Padding = 5;

            Content = _placeholder = new Placeholder
            {
                Text = "No plugins containing this type are installed.",
                ExtraContent = new Button
                {
                    Text = "Open Plugin Manager",
                    Command = new Command((s, e) => App.Current.PluginManagerWindow.Show())
                }
            };

            _mainContent = new Splitter
            {
                Panel1MinimumSize = 150,
                Panel1 = new Scrollable
                {
                    Border = BorderType.None,
                    Content = _sourceSelector = controlBuilder.Build<TypeListBox<TSource>>()
                },
                Panel2 = new Scrollable
                {
                    Content = _settingStoreEditor
                }
            };

            _settingStoreEditor.StoreBinding.Bind(
                _sourceSelector.SelectedItemBinding.Convert(t => StoreCollection?.FromType(t))!
            );

            if (!Platform.IsMac) // Don't do this on macOS, causes poor UI performance.
                _settingStoreEditor.BackgroundColor = SystemColors.WindowBackground;

            _pluginManager.AssembliesChanged += HandleAssembliesChanged;
        }

        private readonly Placeholder _placeholder;
        private readonly Splitter _mainContent;
        private readonly TypeListBox<TSource> _sourceSelector;
        private readonly ToggleablePluginSettingStoreEditor _settingStoreEditor;

        private PluginSettingsCollection _storeCollection;
        public PluginSettingsCollection StoreCollection
        {
            set
            {
                _storeCollection = value;
                OnStoreCollectionChanged();
            }
            get => _storeCollection;
        }

        public event EventHandler<EventArgs> StoreCollectionChanged;

        protected virtual void OnStoreCollectionChanged()
        {
            StoreCollectionChanged?.Invoke(this, EventArgs.Empty);
            RefreshContent();
        }

        private void HandleAssembliesChanged(object? sender, EventArgs e) => Application.Instance.AsyncInvoke(RefreshContent);

        private void RefreshContent()
        {
            var types = _pluginManager.ExportedTypes.Where(t => t.IsAssignableTo(typeof(TSource)));

            // Update DataStore to new types, this refreshes the editor.
            var prevIndex = _sourceSelector.SelectedIndex;
            _sourceSelector.SelectedIndex = -1;
            _sourceSelector.DataStore = types;
            _sourceSelector.SelectedIndex = prevIndex;

            Content = types.Any() ? _mainContent : _placeholder;
        }

        public BindableBinding<PluginSettingStoreCollectionEditor<TSource>, PluginSettingsCollection> StoreCollectionBinding
        {
            get
            {
                return new BindableBinding<PluginSettingStoreCollectionEditor<TSource>, PluginSettingsCollection>(
                    this,
                    c => c.StoreCollection,
                    (c, v) => c.StoreCollection = v,
                    (c, h) => c.StoreCollectionChanged += h,
                    (c, h) => c.StoreCollectionChanged -= h
                );
            }
        }

        private class ToggleablePluginSettingStoreEditor : PluginSettingStoreEditor<TSource>
        {
            private readonly IPluginFactory _pluginFactory;

            public ToggleablePluginSettingStoreEditor(IPluginFactory pluginFactory)
            {
                _pluginFactory = pluginFactory;
            }

            protected override IEnumerable<Control> GetHeaderControlsForStore(PluginSettings store)
            {
                var enableButton = new CheckBox
                {
                    Text = $"Enable {_pluginFactory.GetFriendlyName(store.Path) ?? store.Path}",
                    Checked = store.Enable
                };
                enableButton.CheckedChanged += (sender, e) => store.Enable = enableButton.Checked ?? false;
                yield return enableButton;
            }
        }
    }
}
