using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Output;
using OpenTabletDriver.Tablet;

#nullable enable

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingHandler : IPipelineElement<IDeviceReport>
    {
        private readonly IOutputMode _outputMode;

        public BindingHandler(IOutputMode outputMode)
        {
            _outputMode = outputMode;

            // Force consume all reports from the last element
            var lastElement = _outputMode.Elements?.LastOrDefault() ?? (IPipelineElement<IDeviceReport>)outputMode;
            lastElement.Emit += Consume;
        }

        public ThresholdBindingState? Tip { set; get; }
        public ThresholdBindingState? Eraser { set; get; }

        public Dictionary<int, BindingState?> PenButtons { set; get; } = new Dictionary<int, BindingState?>();
        public Dictionary<int, BindingState?> AuxButtons { set; get; } = new Dictionary<int, BindingState?>();
        public Dictionary<int, BindingState?> MouseButtons { set; get; } = new Dictionary<int, BindingState?>();

        public BindingState? MouseScrollDown { set; get; }
        public BindingState? MouseScrollUp { set; get; }

        public event Action<IDeviceReport>? Emit;

        public void Consume(IDeviceReport report)
        {
            Emit?.Invoke(report);
            HandleBinding(_outputMode.Tablet, report);
        }

        public void HandleBinding(InputDevice device, IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
                HandleTabletReport(device.Properties.Specifications.Pen, tabletReport);
            if (report is IAuxReport auxReport)
                HandleAuxiliaryReport(auxReport);
            if (report is IMouseReport mouseReport)
                HandleMouseReport(mouseReport);
        }

        private void HandleTabletReport(PenSpecifications pen, ITabletReport report)
        {
            float pressurePercent = report.Pressure / (float)pen.MaxPressure * 100f;
            if (report is IEraserReport eraserReport && eraserReport.Eraser)
                Eraser?.Invoke(report, pressurePercent);
            else
                Tip?.Invoke(report, pressurePercent);

            HandleBindingCollection(report, PenButtons, report.PenButtons);
        }

        private void HandleAuxiliaryReport(IAuxReport report)
        {
            HandleBindingCollection(report, AuxButtons, report.AuxButtons);
        }

        private void HandleMouseReport(IMouseReport report)
        {
            HandleBindingCollection(report, MouseButtons, report.MouseButtons);

            MouseScrollDown?.Invoke(report, report.Scroll.Y < 0);
            MouseScrollUp?.Invoke(report, report.Scroll.Y > 0);
        }

        private static void HandleBindingCollection(IDeviceReport report, IDictionary<int, BindingState?> bindings, IList<bool> newStates)
        {
            for (int i = 0; i < newStates.Count; i++)
            {
                if (bindings.TryGetValue(i, out var binding))
                    binding?.Invoke(report, newStates[i]);
            }
        }
    }
}
