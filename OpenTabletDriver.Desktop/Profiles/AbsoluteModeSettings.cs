using System;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class AbsoluteModeSettings : ViewModel
    {
        private AreaSettings display, tablet;
        private bool _lockar, _clipping, _areaLimiting;

        [JsonProperty("Display")]
        public AreaSettings Display
        {
            set => this.RaiseAndSetIfChanged(ref this.display, value);
            get => this.display;
        }

        [JsonProperty("Tablet")]
        public AreaSettings Tablet
        {
            set => this.RaiseAndSetIfChanged(ref this.tablet, value);
            get => this.tablet;
        }

        [JsonProperty("EnableClipping")]
        public bool EnableClipping
        {
            set => RaiseAndSetIfChanged(ref _clipping, value);
            get => _clipping;
        }

        [JsonProperty("EnableAreaLimiting")]
        public bool EnableAreaLimiting
        {
            set => RaiseAndSetIfChanged(ref _areaLimiting, value);
            get => _areaLimiting;
        }

        [JsonProperty("LockAspectRatio")]
        public bool LockAspectRatio
        {
            set => RaiseAndSetIfChanged(ref _lockar, value);
            get => _lockar;
        }

        public static AbsoluteModeSettings GetDefaults(IServiceProvider serviceProvider, DigitizerSpecifications digitizer)
        {
            var virtualScreen = serviceProvider.GetRequiredService<IVirtualScreen>();

            return new AbsoluteModeSettings
            {
                Display = AreaSettings.GetDefaults(virtualScreen),
                Tablet = AreaSettings.GetDefaults(digitizer),
                EnableClipping = true
            };
        }
    }
}
