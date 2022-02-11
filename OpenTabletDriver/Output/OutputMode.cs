using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Output
{
    public abstract class OutputMode : PipelineManager<IDeviceReport>, IOutputMode
    {
        protected OutputMode(InputDevice tablet)
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

        private IList<IPositionedPipelineElement<IDeviceReport>> _preTransformElements = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();
        private IList<IPositionedPipelineElement<IDeviceReport>> _postTransformElements = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();

        public Matrix3x2 TransformationMatrix { protected set; get; }

        public IList<IPositionedPipelineElement<IDeviceReport>> Elements
        {
            set
            {
                _elements = value;

                Passthrough = false;
                DestroyInternalLinks();

                if (Elements != null && Elements.Any())
                {
                    _preTransformElements = GroupElements(Elements, PipelinePosition.PreTransform);
                    _postTransformElements = GroupElements(Elements, PipelinePosition.PostTransform);

                    Action<IDeviceReport> output = OnOutput;

                    if (_preTransformElements.Any() && !_postTransformElements.Any())
                    {
                        _entryElement = _preTransformElements.First();

                        // PreTransform --> Transform --> Output
                        LinkAll(_preTransformElements, this, output);
                    }
                    else if (_postTransformElements.Any() && !_preTransformElements.Any())
                    {
                        _entryElement = this;

                        // Transform --> PostTransform --> Output
                        LinkAll(this, _postTransformElements, output);
                    }
                    else if (_preTransformElements.Any() && _postTransformElements.Any())
                    {
                        _entryElement = _preTransformElements.First();

                        // PreTransform --> Transform --> PostTransform --> Output
                        LinkAll(_preTransformElements, this, _postTransformElements, output);
                    }
                }
                else
                {
                    Passthrough = true;
                    _preTransformElements = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();
                    _postTransformElements = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();
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

            if (_preTransformElements.Any() && !_postTransformElements.Any())
            {
                UnlinkAll(_preTransformElements, this, output);
            }
            else if (_postTransformElements.Any() && !_preTransformElements.Any())
            {
                UnlinkAll(this, _postTransformElements, output);
            }
            else if (_preTransformElements.Any() && _postTransformElements.Any())
            {
                UnlinkAll(_preTransformElements, this, _postTransformElements, output);
            }
        }
    }
}
