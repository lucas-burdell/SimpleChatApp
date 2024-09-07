using SadConsole;
using SadConsole.UI;
using SadRogue.Primitives;

namespace SimpleChat.Helpers
{
    public static class ControlsConsoleExtensions
    {
        public static void FrameTextBoxes(this ControlsConsole controlsConsole)
        {
            foreach (TextboxWithBorder control in controlsConsole.Controls.OfType<TextboxWithBorder>())
            {
                controlsConsole.Surface.DrawBox(control.Bounds.Expand(1, 1), ShapeParameters.CreateStyledBoxThin(Color.White));
                Point stringPos = control.Position + (0, -1);
                controlsConsole.Surface.Print(stringPos.X, stringPos.Y, $" {control.Tag.Title} ");
            }
        }
    }
}
