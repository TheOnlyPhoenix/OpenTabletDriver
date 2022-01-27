using System;
using System.Diagnostics;
using System.IO;
using OpenTabletDriver.Desktop.Interop;

namespace OpenTabletDriver.UX
{
    public class DaemonWatchdog : IDisposable
    {
        public event EventHandler DaemonExited;

        private Process daemonProcess;

        private readonly static ProcessStartInfo startInfo = DesktopInterop.CurrentPlatform switch
        {
            SystemPlatform.Windows => new ProcessStartInfo
            {
                FileName = Path.Join(Directory.GetCurrentDirectory(), "OpenTabletDriver.Daemon.exe"),
                Arguments = "",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                CreateNoWindow = true
            },
            SystemPlatform.MacOS => new ProcessStartInfo
            {
                FileName = Path.Join(AppContext.BaseDirectory, "OpenTabletDriver.Daemon"),
                Arguments = $"-c {Path.Join(AppContext.BaseDirectory, "Configurations")}"
            },
            _ => new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = Path.Join(Directory.GetCurrentDirectory(), "OpenTabletDriver.Daemon.dll")
            }
        };

        public static bool CanExecute =>
            File.Exists(startInfo.FileName) ||
            File.Exists(startInfo.Arguments);

        public void Start()
        {
            this.daemonProcess = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            this.daemonProcess.Exited += (_, e) =>
            {
                DaemonExited?.Invoke(this, EventArgs.Empty);
            };
            this.daemonProcess.Start();
        }

        public void Stop()
        {
            this.daemonProcess?.Kill();
        }

        public void Dispose()
        {
            Stop();
            this.daemonProcess?.Dispose();
        }
    }
}
