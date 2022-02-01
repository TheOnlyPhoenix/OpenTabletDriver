using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class BindingSettings : ViewModel
    {
        private float _tP, _eP;
        private PluginSettingStore _tipButton, _eraserButton, _mouseScrollUp, _mouseScrollDown;
        private PluginSettingStoreCollection _penButtons = new PluginSettingStoreCollection(),
            _auxButtons = new PluginSettingStoreCollection(),
            _mouseButtons = new PluginSettingStoreCollection();

        [JsonProperty("TipActivationThreshold")]
        public float TipActivationThreshold
        {
            set => RaiseAndSetIfChanged(ref _tP, value);
            get => _tP;
        }

        [JsonProperty("TipButton")]
        public PluginSettingStore TipButton
        {
            set => RaiseAndSetIfChanged(ref _tipButton, value);
            get => _tipButton;
        }

        [JsonProperty("EraserActivationThreshold")]
        public float EraserActivationThreshold
        {
            set => RaiseAndSetIfChanged(ref _eP, value);
            get => _eP;
        }

        [JsonProperty("EraserButton")]
        public PluginSettingStore EraserButton
        {
            set => RaiseAndSetIfChanged(ref _eraserButton, value);
            get => _eraserButton;
        }

        [JsonProperty("PenButtons")]
        public PluginSettingStoreCollection PenButtons
        {
            set => RaiseAndSetIfChanged(ref _penButtons, value);
            get => _penButtons;
        }

        [JsonProperty("AuxButtons")]
        public PluginSettingStoreCollection AuxButtons
        {
            set => RaiseAndSetIfChanged(ref _auxButtons, value);
            get => _auxButtons;
        }

        [JsonProperty("MouseButtons")]
        public PluginSettingStoreCollection MouseButtons
        {
            set => RaiseAndSetIfChanged(ref _mouseButtons, value);
            get => _mouseButtons;
        }

        [JsonProperty("MouseScrollUp")]
        public PluginSettingStore MouseScrollUp
        {
            set => RaiseAndSetIfChanged(ref _mouseScrollUp, value);
            get => _mouseScrollUp;
        }

        [JsonProperty("MouseScrollDown")]
        public PluginSettingStore MouseScrollDown
        {
            set => RaiseAndSetIfChanged(ref _mouseScrollDown, value);
            get => _mouseScrollDown;
        }

        public static BindingSettings GetDefaults(TabletSpecifications tabletSpecifications)
        {
            var bindingSettings = new BindingSettings
            {
                TipButton = new PluginSettingStore(
                    typeof(MouseBinding),
                    new
                    {
                        Button = nameof(MouseButton.Left)
                    }
                ),
                PenButtons = new PluginSettingStoreCollection(),
                AuxButtons = new PluginSettingStoreCollection(),
                MouseButtons = new PluginSettingStoreCollection()
            };
            bindingSettings.MatchSpecifications(tabletSpecifications);
            return bindingSettings;
        }

        public void MatchSpecifications(TabletSpecifications tabletSpecifications)
        {
            int penButtonCount = (int?)tabletSpecifications.Pen?.Buttons?.ButtonCount ?? 0;
            int auxButtonCount = (int?)tabletSpecifications.AuxiliaryButtons?.ButtonCount ?? 0;
            int mouseButtonCount = (int?)tabletSpecifications.MouseButtons?.ButtonCount ?? 0;

            PenButtons = PenButtons.SetExpectedCount(penButtonCount);
            AuxButtons = AuxButtons.SetExpectedCount(auxButtonCount);
            MouseButtons = MouseButtons.SetExpectedCount(mouseButtonCount);
        }
    }
}
