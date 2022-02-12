using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Output;
using OpenTabletDriver.Tablet;

#nullable enable

namespace OpenTabletDriver
{
    [PublicAPI]
    public class InputDevice
    {
        public InputDevice(TabletConfiguration configuration, IList<InputDeviceEndpoint> endpoints)
        {
            Configuration = configuration;
            Endpoints = endpoints;

            foreach (var dev in Endpoints)
            {
                // Hook endpoint states
                dev.ConnectionStateChanged += (sender, reading) =>
                {
                    if (_connected && !reading)
                    {
                        _connected = false;
                        Disconnected?.Invoke(this, EventArgs.Empty);
                    }
                };
                dev.Report += HandleReport;
            }
        }

        [JsonConstructor]
        private InputDevice()
        {
            Endpoints = Array.Empty<InputDeviceEndpoint>();
            Configuration = null!;
        }

        private bool _connected = true;

        public event EventHandler<EventArgs>? Disconnected;

        public TabletConfiguration Configuration { protected set; get; }

        [JsonIgnore]
        public IList<InputDeviceEndpoint> Endpoints { get; }

        /// <summary>
        /// The active output mode at the end of the data pipeline for all data to be processed.
        /// </summary>
        [JsonIgnore]
        public IOutputMode? OutputMode { set; get; }

        private void HandleReport(object? sender, IDeviceReport report)
        {
            OutputMode?.Read(report);
        }
    }
}
