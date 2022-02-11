using System.ComponentModel;
using System.Numerics;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Attributes.UI;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Output
{
    /// <summary>
    /// An absolutely positioned output mode.
    /// </summary>
    [PluginIgnore]
    public abstract class AbsoluteOutputMode : OutputMode
    {
        protected AbsoluteOutputMode(InputDevice tablet, IAbsolutePointer absolutePointer)
            : base(tablet)
        {
            Pointer = absolutePointer;
        }

        private Vector2 _min, _max;
        private AngledArea _inputArea;
        private Area _outputArea;

        /// <summary>
        /// The area in which the tablet's input is transformed to.
        /// </summary>
        [Setting("Input Area")]
        [MemberSourcedDefaults(nameof(GetDefaultInputArea), typeof(DigitizerSpecifications))]
        [AspectRatioLock(nameof(Output), nameof(LockAspectRatio))]
        public AngledArea Input
        {
            set
            {
                _inputArea = value;
                TransformationMatrix = CreateTransformationMatrix();
            }
            get => _inputArea;
        }

        /// <summary>
        /// The area in which the final processed output is transformed to.
        /// </summary>
        [Setting("Output Area")]
        [MemberSourcedDefaults(nameof(GetDefaultOutputArea), typeof(IVirtualScreen))]
        [AspectRatioLock(nameof(Input), nameof(LockAspectRatio))]
        public Area Output
        {
            set
            {
                _outputArea = value;
                TransformationMatrix = CreateTransformationMatrix();
            }
            get => _outputArea;
        }

        /// <summary>
        /// Whether to lock aspect ratio when applying area settings.
        /// </summary>
        [Setting("Lock Aspect Ratio"), DefaultValue(false)]
        public bool LockAspectRatio { set; get; }

        /// <summary>
        /// Whether to clip all tablet inputs to the assigned areas.
        /// </summary>
        /// <remarks>
        /// If false, input outside of the area can escape the assigned areas, but still will be transformed.
        /// If true, input outside of the area will be clipped to the edges of the assigned areas.
        /// </remarks>
        [Setting("Area Clipping"), DefaultValue(true)]
        public bool AreaClipping { set; get; }

        /// <summary>
        /// Whether to stop accepting input outside of the assigned areas.
        /// </summary>
        /// <remarks>
        /// If true, <see cref="AreaClipping"/> is automatically implied true.
        /// </remarks>
        [Setting("Area Limiting"), DefaultValue(true)]
        public bool AreaLimiting { set; get; }

        [Setting("Keep inside maximum bounds"), DefaultValue(true)]
        public bool AreaBounds { set; get; }

        /// <summary>
        /// The class in which the final absolute positioned output is handled.
        /// </summary>
        public IAbsolutePointer Pointer { get; }

        protected override Matrix3x2 CreateTransformationMatrix()
        {
            if (Input != null && Output != null && Tablet != null)
            {
                var transform = CalculateTransformation(Input, Output, Tablet.Properties.Specifications.Digitizer);

                var halfDisplayWidth = Output?.Width / 2 ?? 0;
                var halfDisplayHeight = Output?.Height / 2 ?? 0;

                var minX = Output?.Position.X - halfDisplayWidth ?? 0;
                var maxX = Output?.Position.X + Output?.Width - halfDisplayWidth ?? 0;
                var minY = Output?.Position.Y - halfDisplayHeight ?? 0;
                var maxY = Output?.Position.Y + Output?.Height - halfDisplayHeight ?? 0;

                _min = new Vector2(minX, minY);
                _max = new Vector2(maxX, maxY);

                return transform;
            }
            else
            {
                return Matrix3x2.Identity;
            }
        }

        private static Matrix3x2 CalculateTransformation(AngledArea input, Area output, DigitizerSpecifications digitizer)
        {
            // Convert raw tablet data to millimeters
            var res = Matrix3x2.CreateScale(
                digitizer.Width / digitizer.MaxX,
                digitizer.Height / digitizer.MaxY);

            // Translate to the center of input area
            res *= Matrix3x2.CreateTranslation(
                -input.Position.X, -input.Position.Y);

            // Apply rotation
            res *= Matrix3x2.CreateRotation(
                (float)(-input.Rotation * System.Math.PI / 180));

            // Scale millimeters to pixels
            res *= Matrix3x2.CreateScale(
                output.Width / input.Width, output.Height / input.Height);

            // Translate output to virtual screen coordinates
            res *= Matrix3x2.CreateTranslation(
                output.Position.X, output.Position.Y);

            return res;
        }

        /// <summary>
        /// Transposes, transforms, and performs all absolute positioning calculations to a <see cref="IAbsolutePositionReport"/>.
        /// </summary>
        /// <param name="report">The <see cref="IAbsolutePositionReport"/> in which to transform.</param>
        protected override IAbsolutePositionReport Transform(IAbsolutePositionReport report)
        {
            // Apply transformation
            var pos = Vector2.Transform(report.Position, TransformationMatrix);

            // Clipping to display bounds
            var clippedPoint = Vector2.Clamp(pos, _min, _max);
            if (AreaLimiting && clippedPoint != pos)
                return null;

            if (AreaClipping)
                pos = clippedPoint;

            report.Position = pos;

            return report;
        }

        protected override void OnOutput(IDeviceReport report)
        {
            if (report is IEraserReport eraserReport && Pointer is IEraserHandler eraserHandler)
                eraserHandler.SetEraser(eraserReport.Eraser);
            if (report is IAbsolutePositionReport absReport)
                Pointer.SetPosition(absReport.Position);
            if (report is ITabletReport tabletReport && Pointer is IPressureHandler pressureHandler)
                pressureHandler.SetPressure(tabletReport.Pressure / (float)Tablet.Properties.Specifications.Pen.MaxPressure);
            if (report is ITiltReport tiltReport && Pointer is ITiltHandler tiltHandler)
                tiltHandler.SetTilt(tiltReport.Tilt);
            if (report is IProximityReport proximityReport)
            {
                if (Pointer is IProximityHandler proximityHandler)
                    proximityHandler.SetProximity(proximityReport.NearProximity);
                if (Pointer is IHoverDistanceHandler hoverDistanceHandler)
                    hoverDistanceHandler.SetHoverDistance(proximityReport.HoverDistance);
            }
            if (Pointer is ISynchronousPointer synchronousPointer)
            {
                if (report is OutOfRangeReport)
                    synchronousPointer.Reset();
                synchronousPointer.Flush();
            }
        }

        public static AngledArea GetDefaultInputArea(DigitizerSpecifications digitizer)
        {
            return new AngledArea
            {
                Width = digitizer.Width,
                Height = digitizer.Height,
                Position = new Vector2(digitizer.Width / 2, digitizer.Height / 2),
                Rotation = 0
            };
        }

        public static Area GetDefaultOutputArea(IVirtualScreen virtualScreen)
        {
            return new Area
            {
                Width = virtualScreen.Width,
                Height = virtualScreen.Height,
                Position = new Vector2(virtualScreen.Width / 2, virtualScreen.Height / 2)
            };
        }
    }
}
