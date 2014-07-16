using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LudumDare23.Classes
{
    class Button
    {
        int X, Y, Width, Height, Etat = 0;
        private bool mClick;
        Texture2D Texture;
        public event EventHandler Click;


        public Button(int pX, int pY, int pWidth, int pHeight, Texture2D pTexture)
        {
            X = pX;
            Y = pY;
            Width = pWidth;
            Height = pHeight;
            Texture = pTexture;
        }

        public void Update(ref MouseState pMouseState)
        {

            bool click = pMouseState.LeftButton == ButtonState.Pressed, On = false;
            Etat = 0;
            if (click || !click && mClick)
            {
                On = pMouseState.X > X && pMouseState.X < X + Width &&
                    pMouseState.Y > Y && pMouseState.Y < Y + Height;
                if (On)
                    Etat = 1;
            }
            if (On && !click && mClick && Click != null)
                Click(this, EventArgs.Empty);
            mClick = click;
        }
        public void Draw(SpriteBatch pSpriteBatch)
        {
            Rectangle source = new Rectangle(0, Etat * Height, Width, Height);
            pSpriteBatch.Draw(Texture, new Vector2(X, Y), source, Color.White);
        }
    }
}
