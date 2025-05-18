using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace idleGame
{
    public class UIButton
    {
        public Rectangle Bounds;
        public string Label;
        public SpriteFont Font;
        public Texture2D Background;
        public bool IsHovered => Bounds.Contains(Mouse.GetState().Position);
        public bool WasClicked { get; private set; }

        private MouseState previousMouse;

        public UIButton(Rectangle bounds, string label, SpriteFont font, Texture2D background)
        {
            Bounds = bounds;
            Label = label;
            Font = font;
            Background = background;
        }

        public void Update()
        {
            MouseState mouse = Mouse.GetState();
            WasClicked = IsHovered && mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released;
            previousMouse = mouse;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Background, Bounds, IsHovered ? Color.Gray : Color.White);
            Vector2 textSize = Font.MeasureString(Label);
            Vector2 textPos = new Vector2(
                Bounds.Center.X - textSize.X / 2,
                Bounds.Center.Y - textSize.Y / 2
            );
            spriteBatch.DrawString(Font, Label, textPos, Color.Black);
        }
    }
}
