using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.UX.Controls.Generic.Reflection;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class OutputModeEditor : Panel
    {
        private readonly IDriverDaemon _daemon;
        private readonly IPluginManager _pluginManager;
        private readonly IControlGenerator _controlGenerator;

        public OutputModeEditor(
            IDriverDaemon daemon,
            IControlBuilder controlBuilder,
            IControlGenerator controlGenerator,
            IPluginManager pluginManager
        )
        {
            _daemon = daemon;
            _controlGenerator = controlGenerator;
            _pluginManager = pluginManager;

            _outputModeSelector = controlBuilder.Build<TypeDropDown<IOutputMode>>();

            _absoluteModeEditor = controlBuilder.Build<AbsoluteModeEditor>();
            _relativeModeEditor = controlBuilder.Build<RelativeModeEditor>();

            Content = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(_editorContainer, true),
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Control = new Panel
                        {
                            Width = 300,
                            Content = _outputModeSelector
                        }
                    }
                }
            };

            var settingsBinding = ProfileBinding.Child(c => c!.OutputMode);

            _absoluteModeEditor.SettingsBinding.Bind(settingsBinding!);
            _relativeModeEditor.SettingsBinding.Bind(settingsBinding!);

            _outputModeSelector.SelectedItemBinding.Convert(GetSettingsForType, GetTypeForSettings).Bind(settingsBinding!);
            _outputModeSelector.SelectedValueChanged += (sender, e) => UpdateOutputMode(Profile?.OutputMode);

            App.Driver.TabletsChanged += (sender, e) => UpdateTablets(e);
            UpdateTablets();
        }

        private void UpdateTablets(IEnumerable<TabletConfiguration>? tablets = null) => Application.Instance.AsyncInvoke(UpdateTabletsAsync(tablets));

        private async Task UpdateTabletsAsync(IEnumerable<TabletConfiguration>? tablets)
        {
            tablets ??= await _daemon.GetTablets();
            var selectedTablet = tablets.FirstOrDefault(t => t.Name == Profile?.Tablet);
            if (selectedTablet != null)
                SetTabletSize(selectedTablet.Specifications);
        }

        private Profile? _profile;
        public Profile? Profile
        {
            set
            {
                _profile = value;
                OnProfileChanged();
            }
            get => _profile;
        }

        public event EventHandler<EventArgs>? ProfileChanged;

        protected virtual void OnProfileChanged()
        {
            ProfileChanged?.Invoke(this, EventArgs.Empty);
            UpdateTablets();
        }

        public BindableBinding<OutputModeEditor, Profile?> ProfileBinding
        {
            get
            {
                return new BindableBinding<OutputModeEditor, Profile?>(
                    this,
                    c => c.Profile,
                    (c, v) => c.Profile = v,
                    (c, h) => c.ProfileChanged += h,
                    (c, h) => c.ProfileChanged -= h
                );
            }
        }

        private readonly Panel _editorContainer = new Panel();
        private readonly AbsoluteModeEditor _absoluteModeEditor;
        private readonly RelativeModeEditor _relativeModeEditor;
        private readonly TypeDropDown<IOutputMode> _outputModeSelector;

        private readonly Placeholder _noOutputModePlaceholder = new Placeholder
        {
            Text = "No output mode is selected."
        };

        public void SetTabletSize(TabletSpecifications tablet)
        {
            _absoluteModeEditor.SetInputArea(tablet);
        }

        public void SetDisplaySize(IVirtualScreen virtualScreen)
        {
            _absoluteModeEditor.SetOutputArea(virtualScreen);
        }

        private void UpdateOutputMode(PluginSettings? settings)
        {
            if (settings != null)
            {
                var outputMode = GetTypeForSettings(settings);
                var showAbsolute = outputMode?.IsSubclassOf(typeof(AbsoluteOutputMode)) ?? false;
                var showRelative = outputMode?.IsSubclassOf(typeof(RelativeOutputMode)) ?? false;

                if (showAbsolute)
                    _editorContainer.Content = _absoluteModeEditor;
                else if (showRelative)
                    _editorContainer.Content = _relativeModeEditor;
                else if (outputMode != null)
                {
                    var binding = ProfileBinding.Child(c => c!.OutputMode);
                    _editorContainer.Content = _controlGenerator.Generate(outputMode, binding);
                }
                else
                    _editorContainer.Content = _noOutputModePlaceholder;
            }
            else
            {
                _editorContainer.Content = _noOutputModePlaceholder;
            }
        }

        private TypeInfo? GetTypeForSettings(PluginSettings? settings)
        {
            if (settings is null)
                return null;

            return _pluginManager.ExportedTypes.FirstOrDefault(t => t.GetPath() == settings.Path)?.GetTypeInfo();
        }

        private PluginSettings? GetSettingsForType(TypeInfo? type)
        {
            if (type is null)
                return null;

            if (Profile?.OutputMode.Path == type.GetPath())
            {
                return Profile.OutputMode;
            }

            return new PluginSettings(type);
        }
    }
}
