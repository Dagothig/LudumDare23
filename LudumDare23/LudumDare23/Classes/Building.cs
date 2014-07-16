using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare23.Classes
{
    public delegate void CompletionEventHandler(object sender, EventArgs e);
    public enum BuildingType { Bridge, House, Barracks, Workshop, Temple, GreenHouse, Tree, FallingTree};
    class Building
    {
        public int X, Y, Radius, ConstructionLevel = 0;
        public BuildingType Type;
        public Texture2D Texture;
        public event CompletionEventHandler Completion;

        public Building(int pX, int pY, int pRadius, int pConstructionTime, BuildingType pType, Texture2D pTexture)
        {
            X = pX;
            Y = pY;
            Radius = pRadius;
            ConstructionLevel = pConstructionTime;
            Type = pType;
            Texture = pTexture;
        }

        public void Update(List<Person> pPopulace, int pWorkshopCount)
        {

            if (ConstructionLevel > 0)
            {
                if (Type != BuildingType.Tree)
                {
                    Vector2 pos = new Vector2(X, Y);
                    foreach (Person person in pPopulace)
                    {
                        if (person.Praying <= 0)
                        {
                            if (Type != BuildingType.FallingTree && person.Job == Job.Builder && Person.Distance(new Vector2(person.X, person.Y), pos) < Radius)
                                ConstructionLevel -= 1 + pWorkshopCount;
                            else if (Type == BuildingType.FallingTree && person.Job == Job.Villager && Person.Distance(new Vector2(person.X, person.Y), pos) < Radius)
                                ConstructionLevel--;
                        }
                    }
                }
                else
                    ConstructionLevel--;
                if (ConstructionLevel <= 0 && Completion != null)
                    Completion(this, EventArgs.Empty);
            }
        }
        public void Draw(SpriteBatch pSpriteBatch, int pX, bool pPlayerOwned)
        {
            Rectangle source = new Rectangle(0, 0, Texture.Width / 2, Texture.Height / 2);
            if (Type == BuildingType.Tree || Type == BuildingType.FallingTree || Type == BuildingType.Bridge)
            {
                source.X = 0;
                source.Width = Texture.Width;
            }
            else if (pPlayerOwned)
                source.X = 0;
            else
                source.X = source.Width;
            if (ConstructionLevel <= 0)
                source.Y = Texture.Height / 2;
            float buildingsExceptions;
            if (Type == BuildingType.Bridge)
                buildingsExceptions = 0.1f;
            else if (Type == BuildingType.Tree || Type == BuildingType.FallingTree)
                buildingsExceptions = (Y + 6) / 480f;
            else
                buildingsExceptions = (Y + 16) / 480f;
            if (Type != BuildingType.Bridge)
                pSpriteBatch.Draw(Texture, new Vector2(X - pX, Y), source, Color.White, 0, new Vector2(source.Width / 2, source.Height / 2), 1, SpriteEffects.None, buildingsExceptions);
            else
                pSpriteBatch.Draw(Texture, new Vector2(X - pX + 2, Y + 54), source, Color.White, 0, new Vector2(source.Width / 2, source.Height / 2), 1, SpriteEffects.None, buildingsExceptions);
        }
    }
}
