using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using OpenTabletDriver.Output;
using OpenTabletDriver.Tablet;

#nullable enable

namespace OpenTabletDriver
{
    public class InputDevice
    {
        public InputDevice(TabletConfiguration configuration, IList<InputDeviceEndpoint> endpoints)
        {
            Properties = configuration;
            Endpoints = endpoints;

            foreach (var dev in Endpoints)
            {
                // Hook endpoint states
                dev.ConnectionStateChanged += (sender, reading) =>
                {
                    if (this.connected && !reading)
                    {
                        this.connected = false;
                        Disconnected?.Invoke(this, new EventArgs());
                    }
                };
            }
        }

        private bool connected = true;
        private IList<InputDeviceEndpoint> endpoints;

        public event EventHandler<EventArgs>? Disconnected;

        public TabletConfiguration Properties { protected set; get; }
        public IList<InputDeviceEndpoint> Endpoints
        {
            [MemberNotNull(nameof(endpoints))]
            protected set
            {
                this.endpoints = value;
                foreach (var dev in Endpoints)
                    dev.Report += HandleReport;
            }
            get => this.endpoints;
        }

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
