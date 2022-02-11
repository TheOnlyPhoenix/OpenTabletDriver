using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;

#nullable enable

namespace OpenTabletDriver.Desktop.Contracts
{
    public interface IDriverDaemon
    {
        event EventHandler<LogMessage> Message;
        event EventHandler<DebugReportData> DeviceReport;
        event EventHandler<IEnumerable<InputDevice>>? TabletsChanged;

        Task Initialize();

        Task WriteMessage(LogMessage message);

        Task LoadPlugins();
        Task<bool> InstallPlugin(string filePath);
        Task<bool> UninstallPlugin(string friendlyName);
        Task<bool> DownloadPlugin(PluginMetadata metadata);

        Task<IEnumerable<SerializedDeviceEndpoint>> GetDevices();

        Task<IEnumerable<InputDevice>> GetTablets();
        Task<IEnumerable<InputDevice>> DetectTablets();

        Task SetSettings(Settings settings);
        Task<Settings> GetSettings();
        Task ResetSettings();

        Task SetPreset(string name);
        Task<IEnumerable<string>> GetPresets();
        Task SavePreset(string name, Settings settings);

        Task<AppInfo> GetApplicationInfo();
        Task<DiagnosticInfo> GetDiagnostics();

        Task SetTabletDebug(bool isEnabled);
        Task<string> RequestDeviceString(int vendorID, int productID, int index);

        Task<IEnumerable<LogMessage>> GetCurrentLog();

        Task<PluginSettings> GetDefaults(string path);

        Task<TypeProxy> GetProxiedType(string typeName);
        Task<IEnumerable<TypeProxy>> GetMatchingTypes(string typeName);

        Task<bool> HasUpdate();
        Task<Release> GetUpdateInfo();
        Task InstallUpdate();
    }
}
