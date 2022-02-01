using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Output
{
    public abstract class OutputMode : PipelineManager<IDeviceReport>, IOutputMode
    {
        public OutputMode(InputDevice tablet)
        {
            Tablet = tablet;
            Passthrough = true;
        }

        private bool _passthrough;
        private InputDevice _tablet;
        private IList<IPositionedPipelineElement<IDeviceReport>> _elements;
        private IPipelineElement<IDeviceReport> _entryElement;

        public event Action<IDeviceReport> Emit;

        protected bool Passthrough
        {
            private set
            {
                Action<IDeviceReport> output = OnOutput;
                if (value && !_passthrough)
                {
                    _entryElement = this;
                    Link(this, output);
                    _passthrough = true;
                }
                else if (!value && _passthrough)
                {
                    _entryElement = null;
                    Unlink(this, output);
                    _passthrough = false;
                }
            }
            get => _passthrough;
        }

        protected IList<IPositionedPipelineElement<IDeviceReport>> PreTransformElements { private set; get; } = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();
        protected IList<IPositionedPipelineElement<IDeviceReport>> PostTransformElements { private set; get; } = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();

        public Matrix3x2 TransformationMatrix { protected set; get; }

        public IList<IPositionedPipelineElement<IDeviceReport>> Elements
        {
            set
            {
                _elements = value;

                Passthrough = false;
                DestroyInternalLinks();

                if (Elements != null && Elements.Count > 0)
                {
                    PreTransformElements = GroupElements(Elements, PipelinePosition.PreTransform);
                    PostTransformElements = GroupElements(Elements, PipelinePosition.PostTransform);

                    Action<IDeviceReport> output = OnOutput;

                    if (PreTransformElements.Any() && !PostTransformElements.Any())
                    {
                        _entryElement = PreTransformElements.First();

                        // PreTransform --> Transform --> Output
                        LinkAll(PreTransformElements, this, output);
                    }
                    else if (PostTransformElements.Any() && !PreTransformElements.Any())
                    {
                        _entryElement = this;

                        // Transform --> PostTransform --> Output
                        LinkAll(this, PostTransformElements, output);
                    }
                    else if (PreTransformElements.Any() && PostTransformElements.Any())
                    {
                        _entryElement = PreTransformElements.First();

                        // PreTransform --> Transform --> PostTransform --> Output
                        LinkAll(PreTransformElements, this, PostTransformElements, output);
                    }
                }
                else
                {
                    Passthrough = true;
                    PreTransformElements = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();
                    PostTransformElements = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();
                }
            }
            get => _elements;
        }

        public InputDevice Tablet
        {
            set
            {
                _tablet = value;
                TransformationMatrix = CreateTransformationMatrix();
            }
            get => _tablet;
        }

        public virtual void Consume(IDeviceReport report)
        {
            if (report is IAbsolutePositionReport tabletReport)
                if (Transform(tabletReport) is IAbsolutePositionReport transformedReport)
                    report = transformedReport;

            Emit?.Invoke(report);
        }

        public virtual void Read(IDeviceReport deviceReport) => _entryElement?.Consume(deviceReport);

        protected abstract Matrix3x2 CreateTransformationMatrix();
        protected abstract IAbsolutePositionReport Transform(IAbsolutePositionReport tabletReport);
        protected abstract void OnOutput(IDeviceReport report);

        private void DestroyInternalLinks()
        {
            Action<IDeviceReport> output = OnOutput;

            if (PreTransformElements.Any() && !PostTransformElements.Any())
            {
                UnlinkAll(PreTransformElements, this, output);
            }
            else if (PostTransformElements.Any() && !PreTransformElements.Any())
            {
                UnlinkAll(this, PostTransformElements, output);
            }
            else if (PreTransformElements.Any() && PostTransformElements.Any())
            {
                UnlinkAll(PreTransformElements, this, PostTransformElements, output);
            }
        }
    }
}
