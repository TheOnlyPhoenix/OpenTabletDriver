using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using OpenTabletDriver.Components;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Migration;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.SystemDrivers;
using OpenTabletDriver.Tablet;

#nullable enable

namespace OpenTabletDriver.Daemon
{
    public class DriverDaemon : IDriverDaemon
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDriver _driver;
        private readonly ICompositeDeviceHub _deviceHub;
        private readonly IAppInfo _appInfo;
        private readonly IPluginManager _pluginManager;
        private readonly IPluginFactory _pluginFactory;
        private readonly IUpdater? _updater;

        public DriverDaemon(
            IServiceProvider serviceProvider,
            IDriver driver,
            ICompositeDeviceHub deviceHub,
            IAppInfo appInfo,
            IPluginManager pluginManager,
            IPluginFactory pluginFactory
        )
        {
            _serviceProvider = serviceProvider;
            _driver = driver;
            _deviceHub = deviceHub;
            _appInfo = appInfo;
            _pluginManager = pluginManager;
            _pluginFactory = pluginFactory;

            _updater = serviceProvider.GetService<IUpdater>();
        }

        public void Initialize()
        {
            Log.Output += (sender, message) =>
            {
                LogMessages.Add(message);
                Console.WriteLine(Log.GetStringFormat(message));
                Message?.Invoke(sender, message);
            };

            _driver.InputDevicesChanged += TabletsChanged;
            _deviceHub.DevicesChanged += async (sender, args) =>
            {
                if (args.Additions.Any())
                {
                    await DetectTablets();
                    await SetSettings(Settings);
                }
            };

            foreach (var driverInfo in DriverInfo.GetDriverInfos())
            {
                Log.Write("Detect", $"Another tablet driver found: {driverInfo.Name}", LogLevel.Warning);
                if (driverInfo.IsBlockingDriver)
                    Log.Write("Detect", $"Detection for {driverInfo.Name} tablets might be impaired", LogLevel.Warning);
                else if (driverInfo.IsSendingInput)
                    Log.Write("Detect", $"Detected input coming from {driverInfo.Name} driver", LogLevel.Error);
            }

            LoadUserSettings();

#if !DEBUG
            SleepDetection = new(async () =>
            {
                Log.Write(nameof(SleepDetectionThread), "Sleep detected...", LogLevel.Debug);
                await DetectTablets();
            });

            SleepDetection.Start();
#endif
        }

        public event EventHandler<LogMessage>? Message;
        public event EventHandler<DebugReportData>? DeviceReport;
        public event EventHandler<IEnumerable<InputDevice>>? TabletsChanged;

        private Settings? Settings { set; get; }
        private Collection<LogMessage> LogMessages { set; get; } = new Collection<LogMessage>();
        private Collection<ITool> Tools { set; get; } = new Collection<ITool>();
#if !DEBUG
        private readonly SleepDetectionThread SleepDetection;
#endif

        private bool _debugging;

        public Task WriteMessage(LogMessage message)
        {
            Log.Write(message);
            return Task.CompletedTask;
        }

        public Task LoadPlugins()
        {
            _pluginManager.Load();
            return Task.CompletedTask;
        }

        public Task<bool> InstallPlugin(string filePath)
        {
            return Task.FromResult(_pluginManager.InstallPlugin(filePath));
        }

        public Task<bool> UninstallPlugin(string directoryPath)
        {
            var context = _pluginManager.Plugins.First(ctx => ctx.Directory.FullName == directoryPath);
            return Task.FromResult(_pluginManager.UninstallPlugin(context));
        }

        public Task<bool> DownloadPlugin(PluginMetadata metadata)
        {
            return _pluginManager.DownloadPlugin(metadata);
        }

        public Task<IEnumerable<InputDevice>> GetTablets()
        {
            return Task.FromResult(_driver.InputDevices as IEnumerable<InputDevice>);
        }

        public async Task<IEnumerable<InputDevice>> DetectTablets()
        {
            _driver.Detect();

            foreach (var tablet in _driver.InputDevices)
            {
                foreach (var dev in tablet.Endpoints)
                {
                    dev.RawReport += (_, report) => PostDebugReport(report);
                    dev.RawClone = _debugging;
                }
            }

            return await GetTablets();
        }

        public Task SetSettings(Settings? settings)
        {
            // Dispose filters that implement IDisposable interface
            foreach (var obj in _driver.InputDevices.SelectMany(d => d.OutputMode?.Elements ?? (IEnumerable<object>)Array.Empty<object>()))
                if (obj is IDisposable disposable)
                    disposable.Dispose();

            Settings = settings ?? Settings.GetDefaults(_serviceProvider);

            foreach (var device in _driver.InputDevices)
            {
                string group = device.Properties.Name;
                var profile = Settings.Profiles[device];

                profile.BindingSettings.MatchSpecifications(device.Properties.Specifications);

                device.OutputMode = _pluginFactory.Construct<IOutputMode>(profile.OutputMode, device);

                if (device.OutputMode != null)
                {
                    var outputModeName = _pluginFactory.GetName(profile.OutputMode);
                    Log.Write(group, $"Output mode: {outputModeName}");
                }

                if (device.OutputMode is AbsoluteOutputMode absoluteMode)
                    SetAbsoluteModeSettings(device, absoluteMode, profile.AbsoluteModeSettings);

                if (device.OutputMode is RelativeOutputMode relativeMode)
                    SetRelativeModeSettings(device, relativeMode, profile.RelativeModeSettings);

                if (device.OutputMode is IOutputMode outputMode)
                {
                    SetOutputModeSettings(device, outputMode, profile);
                    SetBindingHandlerSettings(device, outputMode, profile.BindingSettings);
                }
            }

            Log.Write("Settings", "Driver is enabled.");

            SetToolSettings();

            return Task.CompletedTask;
        }

        public async Task ResetSettings()
        {
            await SetSettings(Settings.GetDefaults(_serviceProvider));
        }

        private async void LoadUserSettings()
        {
            _pluginManager.Clean();
            await LoadPlugins();
            await DetectTablets();

            var appdataDir = new DirectoryInfo(_appInfo.AppDataDirectory);
            if (!appdataDir.Exists)
            {
                appdataDir.Create();
                Log.Write("Settings", $"Created OpenTabletDriver application data directory: {appdataDir.FullName}");
            }

            var settingsFile = new FileInfo(_appInfo.SettingsFile);

            if (settingsFile.Exists)
            {
                var migrator = new SettingsMigrator(_serviceProvider);
                migrator.Migrate(_appInfo);

                var settings = Settings.Deserialize(settingsFile);
                if (settings != null)
                {
                    await SetSettings(settings);
                }
                else
                {
                    Log.Write("Settings", "Invalid settings detected. Attempting recovery.", LogLevel.Error);
                    settings = Settings.GetDefaults(_serviceProvider);

                    Settings.Recover(settingsFile, settings);
                    Log.Write("Settings", "Recovery complete");
                    await SetSettings(settings);
                }
            }
            else
            {
                await ResetSettings();
            }
        }

        private void SetOutputModeSettings(InputDevice dev, IOutputMode outputMode, Profile profile)
        {
            string group = dev.Properties.Name;

            var elements = from store in profile.Filters
                           where store.Enable
                           let filter = _pluginFactory.Construct<IPositionedPipelineElement<IDeviceReport>>(store, dev)
                           where filter != null
                           select filter;

            outputMode.Elements = elements.ToList();

            if (outputMode.Elements.Any())
                Log.Write(group, $"Filters: {string.Join(", ", outputMode.Elements)}");
        }

        private void SetAbsoluteModeSettings(InputDevice dev, AbsoluteOutputMode absoluteMode, AbsoluteModeSettings settings)
        {
            string group = dev.Properties.Name;
            absoluteMode.Output = settings.Display.Area;

            Log.Write(group, $"Display area: {absoluteMode.Output}");

            absoluteMode.Input = settings.Tablet.Area;
            Log.Write(group, $"Tablet area: {absoluteMode.Input}");

            absoluteMode.AreaClipping = settings.EnableClipping;
            Log.Write(group, $"Clipping: {(absoluteMode.AreaClipping ? "Enabled" : "Disabled")}");

            absoluteMode.AreaLimiting = settings.EnableAreaLimiting;
            Log.Write(group, $"Ignoring reports outside area: {(absoluteMode.AreaLimiting ? "Enabled" : "Disabled")}");
        }

        private void SetRelativeModeSettings(InputDevice dev, RelativeOutputMode relativeMode, RelativeModeSettings settings)
        {
            string group = dev.Properties.Name;
            relativeMode.Sensitivity = settings.Sensitivity;

            Log.Write(group, $"Relative Mode Sensitivity (X, Y): {relativeMode.Sensitivity}");

            relativeMode.Rotation = settings.RelativeRotation;
            Log.Write(group, $"Relative Mode Rotation: {relativeMode.Rotation}");

            relativeMode.ResetTime = settings.ResetTime;
            Log.Write(group, $"Reset time: {relativeMode.ResetTime}");
        }

        private void SetBindingHandlerSettings(InputDevice dev, IOutputMode outputMode, BindingSettings settings)
        {
            string group = dev.Properties.Name;
            var bindingHandler = new BindingHandler(outputMode);

            object? pointer = outputMode switch
            {
                AbsoluteOutputMode absoluteOutputMode => absoluteOutputMode.Pointer,
                RelativeOutputMode relativeOutputMode => relativeOutputMode.Pointer,
                _ => null
            };

            // TODO: Possible null ref, needs fixed
            var mouseButtonHandler = pointer as IMouseButtonHandler;

            var tip = bindingHandler.Tip = new ThresholdBindingState(dev)
            {
                Binding = _pluginFactory.Construct<IBinding>(settings.TipButton, dev, mouseButtonHandler),
                ActivationThreshold = settings.TipActivationThreshold
            };

            if (tip.Binding != null)
            {
                Log.Write(group, $"Tip Binding: [{tip.Binding}]@{tip.ActivationThreshold}%");
            }

            var eraser = bindingHandler.Eraser = new ThresholdBindingState(dev)
            {
                Binding = _pluginFactory.Construct<IBinding>(settings.EraserButton, dev, mouseButtonHandler),
                ActivationThreshold = settings.EraserActivationThreshold
            };

            if (eraser.Binding != null)
            {
                Log.Write(group, $"Eraser Binding: [{eraser.Binding}]@{eraser.ActivationThreshold}%");
            }

            if (settings.PenButtons != null && settings.PenButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(_pluginFactory, dev, settings.PenButtons, bindingHandler.PenButtons, mouseButtonHandler);
                Log.Write(group, $"Pen Bindings: " + string.Join(", ", bindingHandler.PenButtons.Select(b => b.Value?.Binding)));
            }

            if (settings.AuxButtons != null && settings.AuxButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(_pluginFactory, dev, settings.AuxButtons, bindingHandler.AuxButtons, mouseButtonHandler);
                Log.Write(group, $"Express Key Bindings: " + string.Join(", ", bindingHandler.AuxButtons.Select(b => b.Value?.Binding)));
            }

            if (settings.MouseButtons != null && settings.MouseButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(_pluginFactory, dev, settings.MouseButtons, bindingHandler.MouseButtons, mouseButtonHandler);
                Log.Write(group, $"Mouse Button Bindings: [" + string.Join("], [", bindingHandler.MouseButtons.Select(b => b.Value?.Binding)) + "]");
            }

            var scrollUp = bindingHandler.MouseScrollUp = new BindingState
            {
                Binding = _pluginFactory.Construct<IBinding>(settings.MouseScrollUp, dev)
            };

            var scrollDown = bindingHandler.MouseScrollDown = new BindingState
            {
                Binding = _pluginFactory.Construct<IBinding>(settings.MouseScrollDown, dev)
            };

            if (scrollUp.Binding != null || scrollDown.Binding != null)
            {
                Log.Write(group, $"Mouse Scroll: Up: [{scrollUp?.Binding}] Down: [{scrollDown?.Binding}]");
            }
        }

        private void SetBindingHandlerCollectionSettings(IPluginFactory pluginFactory, InputDevice dev, PluginSettingStoreCollection collection, Dictionary<int, BindingState?> targetDict, params object[] args)
        {
            var additionalArgs = args.Append(dev).ToArray();
            for (int index = 0; index < collection.Count; index++)
            {
                var binding = pluginFactory.Construct<IBinding>(collection[index], additionalArgs);
                var state = binding == null ? null : new BindingState
                {
                    Binding = binding
                };

                if (!targetDict.TryAdd(index, state))
                    targetDict[index] = state;
            }
        }

        private void SetToolSettings()
        {
            foreach (var runningTool in Tools)
                runningTool.Dispose();
            Tools.Clear();

            if (Settings != null)
            {
                foreach (PluginSettingStore store in Settings.Tools)
                {
                    if (store.Enable == false)
                        continue;

                    var tool = _pluginFactory.Construct<ITool>(store);

                    if (tool?.Initialize() ?? false)
                    {
                        Tools.Add(tool);
                    }
                    else
                    {
                        var name = _pluginFactory.GetName(store);
                        Log.Write("Tool", $"Failed to initialize {name} tool.", LogLevel.Error);
                    }
                }
            }
        }

        public Task<Settings?> GetSettings()
        {
            return Task.FromResult(Settings);
        }

        public Task<IEnumerable<SerializedDeviceEndpoint>> GetDevices()
        {
            return Task.FromResult(_deviceHub.GetDevices().Select(d => new SerializedDeviceEndpoint(d)));
        }

        public Task<IAppInfo> GetApplicationInfo()
        {
            return Task.FromResult(_appInfo);
        }

        public Task SetTabletDebug(bool enabled)
        {
            _debugging = enabled;
            foreach (var endpoint in _driver.InputDevices.SelectMany(d => d.Endpoints))
            {
                endpoint.RawClone = _debugging;
            }

            Log.Debug("Tablet", $"Tablet debugging is {(_debugging ? "enabled" : "disabled")}");

            return Task.CompletedTask;
        }

        public Task<string> RequestDeviceString(int vid, int pid, int index)
        {
            var tablet = _deviceHub.GetDevices().FirstOrDefault(d => d.VendorID == vid && d.ProductID == pid);
            if (tablet == null)
                throw new IOException($"Device not found ({vid:X2}:{pid:X2})");

            return Task.FromResult(tablet.GetDeviceString((byte)index));
        }

        public Task<IEnumerable<LogMessage>> GetCurrentLog()
        {
            return Task.FromResult((IEnumerable<LogMessage>)LogMessages);
        }

        private void PostDebugReport(IDeviceReport report)
        {
            DeviceReport?.Invoke(this, new DebugReportData(report));
        }

        public Task<bool> HasUpdate()
        {
            return _updater?.CheckForUpdates() ?? Task.FromResult(false);
        }

        public async Task<Release> GetUpdateInfo()
        {
            return await _updater.GetRelease()!;
        }

        public Task InstallUpdate()
        {
            return _updater?.InstallUpdate() ?? Task.CompletedTask;
        }
    }
}
