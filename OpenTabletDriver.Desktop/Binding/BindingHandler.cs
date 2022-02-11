using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop.Migration.LegacySettings.V6;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

#nullable enable

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingHandler : IPipelineElement<IDeviceReport>
    {
        private readonly InputDevice _device;

        public BindingHandler(IServiceProvider serviceProvider, InputDevice device, BindingSettings settings)
        {
            _device = device;

            var outputMode = _device.OutputMode!;

            // Force consume all reports from the last element
            var lastElement = outputMode.Elements?.LastOrDefault() ?? (IPipelineElement<IDeviceReport>)outputMode;
            lastElement.Emit += Consume;

            object? pointer = outputMode switch
            {
                AbsoluteOutputMode absoluteOutputMode => absoluteOutputMode.Pointer,
                RelativeOutputMode relativeOutputMode => relativeOutputMode.Pointer,
                _ => null
            };

            // TODO: Possible null ref, needs fixed
            var mouseButtonHandler = (pointer as IMouseButtonHandler)!;

            Tip = CreateBindingState<ThresholdBindingState>(serviceProvider, settings.TipButton, device, mouseButtonHandler);

            if (Tip != null)
                Tip.ActivationThreshold = settings.TipActivationThreshold;

            Eraser = CreateBindingState<ThresholdBindingState>(serviceProvider, settings.EraserButton, device, mouseButtonHandler);
            if (Eraser != null)
                Eraser.ActivationThreshold = settings.EraserActivationThreshold;

            PenButtons = CreateBindingStates(serviceProvider, device, settings.PenButtons, device, mouseButtonHandler);
            AuxButtons = CreateBindingStates(serviceProvider, device, settings.AuxButtons, device, mouseButtonHandler);
            MouseButtons = CreateBindingStates(serviceProvider, device, settings.MouseButtons, device, mouseButtonHandler);

            MouseScrollDown = CreateBindingState<BindingState>(serviceProvider, settings.MouseScrollDown, device, mouseButtonHandler);
            MouseScrollUp = CreateBindingState<BindingState>(serviceProvider, settings.MouseScrollUp, device, mouseButtonHandler);
        }

        private ThresholdBindingState? Tip { get; }
        private ThresholdBindingState? Eraser { get; }

        private Dictionary<int, BindingState?> PenButtons { get; }
        private Dictionary<int, BindingState?> AuxButtons { get; }
        private Dictionary<int, BindingState?> MouseButtons { get; }

        private BindingState? MouseScrollDown { get; }
        private BindingState? MouseScrollUp { get; }

        public event Action<IDeviceReport>? Emit;

        public void Consume(IDeviceReport report)
        {
            Emit?.Invoke(report);
            HandleBinding(_device.OutputMode!.Tablet, report);
        }

        private void HandleBinding(InputDevice device, IDeviceReport report)
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

        private static T? CreateBindingState<T>(IServiceProvider serviceProvider, PluginSettings? settings, params object[] args) where T : BindingState
        {
            if (settings == null)
                return null;

            var ctor = typeof(T).GetConstructors().First();

            var parameters = from parameter in ctor.GetParameters()
                let type = parameter.ParameterType
                select serviceProvider.GetService(type) ?? args.Append(settings).First(a => a.GetType().IsAssignableTo(type));

            return (T) ctor.Invoke(parameters.ToArray());
        }

        private static Dictionary<int, BindingState?> CreateBindingStates(IServiceProvider serviceProvider, InputDevice dev, PluginSettingsCollection collection, params object[] args)
        {
            var dict = new Dictionary<int, BindingState?>();

            for (int index = 0; index < collection.Count; index++)
            {
                var state = CreateBindingState<BindingState>(serviceProvider, collection[index], args);

                if (!dict.TryAdd(index, state))
                    dict[index] = state;
            }

            return dict;
        }
    }
}
