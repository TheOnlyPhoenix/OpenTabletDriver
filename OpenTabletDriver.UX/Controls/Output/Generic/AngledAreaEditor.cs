using System.Numerics;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Utilities;

namespace OpenTabletDriver.UX.Controls.Output.Generic
{
    public class AngledAreaEditor : AreaEditor<AngledArea>
    {
        public AngledAreaEditor()
        {
            var rotation = new FloatNumberBox();

            SettingsPanel.Items.Add(
                new StackLayoutItem
                {
                    Control = new UnitGroup
                    {
                        Text = "Rotation",
                        Unit = "°",
                        ToolTip = "Angle of rotation about the center of the area.",
                        Orientation = Orientation.Horizontal,
                        Content = rotation
                    }
                }
            );

            var rotationBinding = AreaBinding.Child(c => c.Rotation);
            rotation.ValueBinding.Bind(rotationBinding);
        }

        protected override void CreateMenu()
        {
            base.CreateMenu();

            ContextMenu.Items.GetSubmenu("Flip").Items.Add(
                new ActionCommand
                {
                    MenuText = "Handedness",
                    Action = () =>
                    {
                        Area.Rotation += 180;
                        Area.Rotation %= 360;
                        var x = FullAreaBounds.Width - Area.Position.X;
                        var y = FullAreaBounds.Height - Area.Position.Y;
                        Area.Position = new Vector2(x, y);
                    }
                }
            );
        }
    }
}
