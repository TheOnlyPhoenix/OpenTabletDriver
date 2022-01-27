using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using IDeviceReport = OpenTabletDriver.Tablet.IDeviceReport;
using IOutputMode = OpenTabletDriver.Output.IOutputMode;
using TabletConfiguration = OpenTabletDriver.Tablet.TabletConfiguration;
using TabletReference = OpenTabletDriver.Tablet.TabletReference;

#nullable enable

namespace OpenTabletDriver
{
    public class InputDeviceTree
    {
        public InputDeviceTree(TabletConfiguration configuration, IList<InputDevice> inputDevices)
        {
            Properties = configuration;
            InputDevices = inputDevices;

            foreach (var dev in InputDevices)
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
        private IList<InputDevice> inputDevices;

        public event EventHandler<EventArgs>? Disconnected;

        public TabletConfiguration Properties { protected set; get; }
        public IList<InputDevice> InputDevices
        {
            [MemberNotNull(nameof(inputDevices))]
            protected set
            {
                this.inputDevices = value;
                foreach (var dev in InputDevices)
                    dev.Report += HandleReport;
            }
            get => this.inputDevices;
        }

        /// <summary>
        /// The active output mode at the end of the data pipeline for all data to be processed.
        /// </summary>
        public IOutputMode? OutputMode { set; get; }

        public TabletReference CreateReference() => new TabletReference(Properties, InputDevices.Select(c => c.Identifier));

        private void HandleReport(object? sender, IDeviceReport report)
        {
            OutputMode?.Read(report);
        }

        public static implicit operator TabletReference(InputDeviceTree deviceGroup) => deviceGroup.CreateReference();
    }
}
