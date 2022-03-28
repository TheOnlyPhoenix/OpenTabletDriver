using System;
using Eto.Drawing;

namespace OpenTabletDriver.UX.Controls.Generic
{
    /// <summary>
    /// Workaround for memory leaks on macos.
    /// Use shared FormattedText to draw text.
    /// </summary>
    public class TextDrawer : FormattedText
    {
        public void DrawText(Graphics graphics, Font font, Brush brush, PointF location, String text)
        {
            Text = text;
            Font = font;
            ForegroundBrush = brush;
            graphics.DrawText(this, location);
        }
    }
}
