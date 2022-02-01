using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Components;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.RPC;

namespace OpenTabletDriver.Daemon
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            using (var instance = new Instance("OpenTabletDriver.Daemon"))
            {
                if (instance.AlreadyExists)
                {
                    Console.WriteLine("OpenTabletDriver Daemon is already running.");
                    Thread.Sleep(1000);
                    return;
                }

                var daemon = BuildDaemon(out var serviceProvider);
                var appInfo = serviceProvider.GetRequiredService<IAppInfo>();

                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    var exception = (Exception)e.ExceptionObject;
                    File.WriteAllLines(Path.Join(appInfo.AppDataDirectory, "daemon.log"),
                        new string[]
                        {
                            DateTime.Now.ToString(CultureInfo.InvariantCulture),
                            exception.GetType().FullName,
                            exception.Message,
                            exception.Source,
                            exception.StackTrace,
                            exception.TargetSite?.Name
                        }
                    );
                };

                var rootCommand = new RootCommand("OpenTabletDriver")
                {
                    new Option(new[] { "--appdata", "-a" }, "Application data directory")
                    {
                        Argument = new Argument<DirectoryInfo>("appdata")
                    },
                    new Option(new[] { "--config", "-c" }, "Configuration directory")
                    {
                        Argument = new Argument<DirectoryInfo> ("config")
                    }
                };
                rootCommand.Handler = CommandHandler.Create<DirectoryInfo, DirectoryInfo>((appdata, config) =>
                {
                    if (!string.IsNullOrWhiteSpace(appdata?.FullName))
                        appInfo.AppDataDirectory = appdata.FullName;
                    if (!string.IsNullOrWhiteSpace(config?.FullName))
                        appInfo.ConfigurationDirectory = config.FullName;
                });
                await rootCommand.InvokeAsync(args);

                var rpcHost = new RpcHost<DriverDaemon>("OpenTabletDriver.Daemon");
                rpcHost.ConnectionStateChanged += (sender, state) =>
                    Log.Write("IPC", $"{(state ? "Connected to" : "Disconnected from")} a client.", LogLevel.Debug);

                daemon.Initialize();
                await rpcHost.Run(daemon);
            }
        }

        private static DriverDaemon BuildDaemon(out IServiceProvider serviceProvider)
        {
            return new DaemonBuilder().Build(out serviceProvider);
        }
    }
}
