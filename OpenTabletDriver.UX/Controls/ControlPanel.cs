using System;
using System.Linq;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.UX.Controls.Bindings;
using OpenTabletDriver.UX.Controls.Output;

namespace OpenTabletDriver.UX.Controls
{
    public class ControlPanel : Panel
    {
        private readonly IDriverDaemon _driverDaemon;

        public ControlPanel(IDriverDaemon driverDaemon, IVirtualScreen virtualScreen, IControlBuilder controlBuilder)
        {
            _driverDaemon = driverDaemon;

            _outputModeEditor = controlBuilder.Build<OutputModeEditor>();

            _auxBindingEditor = controlBuilder.Build<AuxiliaryBindingEditor>();
            _mouseBindingEditor = controlBuilder.Build<MouseBindingEditor>();

            this.Content = _tabControl = new TabControl
            {
                Pages =
                {
                    new TabPage
                    {
                        Text = "Output",
                        Content = _outputModeEditor
                    },
                    new TabPage
                    {
                        Text = "Filters",
                        Padding = 5,
                        Content = _filterEditor = controlBuilder.Build<PluginSettingStoreCollectionEditor<IPositionedPipelineElement<IDeviceReport>>>()
                    },
                    new TabPage
                    {
                        Text = "Pen Settings",
                        Content = _penBindingEditor = controlBuilder.Build<PenBindingEditor>()
                    },
                    new TabPage
                    {
                        Text = "Auxiliary Settings",
                        Content = _auxBindingEditor
                    },
                    new TabPage
                    {
                        Text = "Mouse Settings",
                        Content = _mouseBindingEditor
                    },
                    new TabPage
                    {
                        Text = "Tools",
                        Padding = 5,
                        Content = _toolEditor = controlBuilder.Build<PluginSettingStoreCollectionEditor<ITool>>()
                    },
                    new TabPage
                    {
                        Text = "Info",
                        Padding = 5,
                        Content = _placeholder = new Placeholder
                        {
                            Text = "No tablets are detected."
                        }
                    },
                    new TabPage
                    {
                        Text = "Console",
                        Padding = 5,
                        Content = _logView = new LogView()
                    }
                }
            };

            _outputModeEditor.ProfileBinding.Bind(ProfileBinding);
            _penBindingEditor.ProfileBinding.Bind(ProfileBinding);
            _auxBindingEditor.ProfileBinding.Bind(ProfileBinding);
            _mouseBindingEditor.ProfileBinding.Bind(ProfileBinding);
            _filterEditor.StoreCollectionBinding.Bind(ProfileBinding.Child(p => p.Filters));
            _toolEditor.StoreCollectionBinding.Bind(App.Current, a => a.Settings.Tools);

            _outputModeEditor.SetDisplaySize(virtualScreen);

            Log.Output += (_, message) => Application.Instance.AsyncInvoke(() =>
            {
                if (message.Level > LogLevel.Info)
                {
                    _tabControl.SelectedPage = _logView.Parent as TabPage;
                }
            });
        }

        private readonly TabControl _tabControl;
        private readonly Placeholder _placeholder;
        private readonly LogView _logView;
        private readonly OutputModeEditor _outputModeEditor;
        private readonly BindingEditor _penBindingEditor, _auxBindingEditor, _mouseBindingEditor;
        private readonly PluginSettingStoreCollectionEditor<IPositionedPipelineElement<IDeviceReport>> _filterEditor;
        private readonly PluginSettingStoreCollectionEditor<ITool> _toolEditor;

        private Profile? _profile;
        public Profile? Profile
        {
            set
            {
                this._profile = value;
                this.OnProfileChanged();
            }
            get => this._profile;
        }

        public event EventHandler<EventArgs> ProfileChanged;

        protected virtual void OnProfileChanged() => Application.Instance.AsyncInvoke(OnProfileChangedAsync);

        private async Task OnProfileChangedAsync()
        {
            ProfileChanged?.Invoke(this, EventArgs.Empty);

            var tablets = await _driverDaemon.GetTablets();
            var tablet = Profile != null ? tablets.FirstOrDefault(t => t.Name == Profile.Tablet) : null;

            if (Platform.IsMac) _tabControl.Pages.Clear();

            if (tablet != null)
            {
                bool switchToOutput = _tabControl.SelectedPage == _placeholder.Parent;

                SetPageVisibility(_placeholder, false);
                SetPageVisibility(_outputModeEditor, true);
                SetPageVisibility(_filterEditor, true);
                SetPageVisibility(_penBindingEditor, tablet.Specifications.Pen != null);
                SetPageVisibility(_auxBindingEditor, tablet.Specifications.AuxiliaryButtons != null);
                SetPageVisibility(_mouseBindingEditor, tablet.Specifications.MouseButtons != null);
                SetPageVisibility(_toolEditor, true);

                if (switchToOutput) _tabControl.SelectedIndex = 0;
            }
            else
            {
                SetPageVisibility(_placeholder, true);
                SetPageVisibility(_outputModeEditor, false);
                SetPageVisibility(_filterEditor, false);
                SetPageVisibility(_penBindingEditor, false);
                SetPageVisibility(_auxBindingEditor, false);
                SetPageVisibility(_mouseBindingEditor, false);
                SetPageVisibility(_toolEditor, false);

                if (_tabControl.SelectedPage != _logView.Parent) _tabControl.SelectedIndex = 0;
            }

            SetPageVisibility(_logView, true);
        }

        public BindableBinding<ControlPanel, Profile?> ProfileBinding
        {
            get
            {
                return new BindableBinding<ControlPanel, Profile?>(
                    this,
                    c => c.Profile,
                    (c, v) => c.Profile = v,
                    (c, h) => c.ProfileChanged += h,
                    (c, h) => c.ProfileChanged -= h
                );
            }
        }

        private void SetPageVisibility(Control control, bool visible)
        {
            // This works around a bug in Eto.Forms with TabPage visibility
            // https://github.com/picoe/Eto/issues/1224
            if (Platform.IsMac)
            {
                if (visible)
                {
                    var page = control.Parent as TabPage;
                    _tabControl.Pages.Add(page);
                }
            }
            else
            {
                control.Parent.Visible = visible;
            }
        }
    }
}
