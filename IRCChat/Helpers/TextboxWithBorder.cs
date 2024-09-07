using SadConsole;
using SadConsole.UI.Controls;
using Color = SadRogue.Primitives.Color;
using Point = SadRogue.Primitives.Point;

namespace SimpleChat.Helpers
{
    internal class TextboxWithBorder : TextBox
    {
        public new TextboxWithBorderTag Tag { get; set; }

        public TextboxWithBorder(int width, string name, string initialText, Point position) : base(width)
        {
            Tag = new TextboxWithBorderTag()
            {
                Title = name
            };
            Position = position;
            Text = initialText;
        }

        public class TextboxWithBorderTag
        {
            public string Title { get; set; }
        }

        public void FrameTextBoxes()
        {
            Surface.DrawBox(Bounds.Expand(1, 1), ShapeParameters.CreateStyledBoxThin(Color.White));
            Point stringPos = Position + (0, -1);
            Surface.Print(stringPos.X, stringPos.Y, $" {Tag.Title} ");
        }
    }
}
