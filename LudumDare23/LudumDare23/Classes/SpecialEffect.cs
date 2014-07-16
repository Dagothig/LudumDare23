using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare23.Classes
{
    class SpecialEffect
    {
        public int X, Y, IdX = 0, IdY, untilSwitch = 3;
        public Texture2D Texture;
        public int Steps = 6;
        bool Lightning;

        public SpecialEffect(int pX, int pY, int pIdY, Texture2D pTexture, bool pLightning)
        {
            Lightning = pLightning;
            if (Lightning)
                Steps = 3;
            X = pX;
            Y = pY;
            IdY = pIdY;
            Texture = pTexture;
        }

        public void Draw(SpriteBatch pSpriteBatch, int pX)
        {
            if (!Lightning)
                pSpriteBatch.Draw(Texture, new Vector2((int)X - pX, (int)Y), new Rectangle((int)IdX * 8, (int)IdY * 12, 8, 12), Color.White, 0, new Vector2(4, 10), 1, SpriteEffects.None, ((Y + 1) / 480f));
            else
                pSpriteBatch.Draw(Texture, new Vector2((int)X - pX, (int)Y), new Rectangle(0, 0, Texture.Width, Texture.Height), Color.White, 0, new Vector2(Texture.Width/2, Texture.Height - 4), 1, SpriteEffects.None, ((Y + 1) / 480f));
        }
        public void Update()
        {
            if (untilSwitch <= 0)
            {
                IdX++;
                untilSwitch = 3;
            }
            untilSwitch--;
        }
    }
}
