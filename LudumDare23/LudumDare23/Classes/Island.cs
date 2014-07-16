using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LudumDare23.Classes
{
    class Island
    {
        public int X, Y, ZoneX, ZoneY, ZoneWidth = 320, ZoneHeight = 120;
        public int TreeCount
        {
            get
            {
                int count = 0;
                foreach (Building building in Buildings)
                    if (building.Type == BuildingType.Tree || building.Type == BuildingType.FallingTree)
                        count++;
                return count;
            }
        }
        public List<Building> Buildings = new List<Building>();
        Texture2D Texture;
        Texture2D Clouds;

        public Island(int pX, int pY, Texture2D pTexture, Texture2D pClouds, Texture2D pTree, Random pRandom)
        {
            Texture = pTexture;
            Clouds = pClouds;
            X = pX;
            ZoneX = X + 250;
            Y = pY;
            ZoneY = Y + 65;
            bool validPosition;
            for (int index = 0; index < pRandom.Next(50); index++)
            {
                validPosition = true;
                Point pos = Person.GetRandomPoint(this, pRandom, TinyReligion.TreeRadius / 3);
                foreach (Building building in Buildings)
                    if (Person.Distance(new Vector2(building.X, building.Y), new Vector2(pos.X, pos.Y)) < building.Radius / 5 + TinyReligion.TreeRadius / 5)
                        validPosition = false;
                if (validPosition)
                {
                    Buildings.Add(new Building(pos.X, pos.Y, TinyReligion.TreeRadius, 0, BuildingType.Tree, pTree));
                }
            }
        }
        public Island(List<Island> pIslands, int pX, int pY, int pFactor, Texture2D pTexture, Texture2D pClouds, Texture2D pTree, Texture2D pVillager, Texture2D pBuilder, Texture2D pWarrior, Texture2D pPriest, Texture2D pHouse, Texture2D pWorkshop, Texture2D pTemple, Texture2D pBarracks, Texture2D pBridge, Populace pPopulace, Random pRandom)
        {
            Texture = pTexture;
            Clouds = pClouds;
            X = pX;
            ZoneX = X + 250;
            Y = pY;
            ZoneY = Y + 65;
            Person person;
            pPopulace.OwnedIslands.Add(this);
            #region People

            int amountVillagers = pRandom.Next(1, 5 + pFactor*3), index;
            for (index = 0; index < amountVillagers; index++)
            {
                person = new Person(pX + 250 + pRandom.Next(-12, 12), pY + 52 + pRandom.Next(-12, 12), pVillager, this, pPopulace, false);
                person.Job = Job.Villager;
                pPopulace.People.Add(person);
            }
            int amountWarriors = pRandom.Next(1, 4 + pFactor*3);
            for (index = 0; index < amountWarriors; index++)
            {
                person = new Person(pX + 250 + pRandom.Next(-12, 12), pY + 52 + pRandom.Next(-12, 12), pWarrior, this, pPopulace, false);
                person.Job = Job.Warrior;
                pPopulace.People.Add(person);
            }
            int amountPriests = pRandom.Next(1, 4 + pFactor*3);
            for (index = 0; index < amountPriests; index++)
            {
                person = new Person(pX + 250 + pRandom.Next(-12, 12), pY + 52 + pRandom.Next(-12, 12), pPriest, this, pPopulace, false);
                person.Job = Job.Priest;
                pPopulace.People.Add(person);
            }
            int amountCraftsmen = pRandom.Next(1, 3 + pFactor*3);
            for (index = 0; index < amountCraftsmen; index++)
            {
                person = new Person(pX + 250 + pRandom.Next(-12, 12), pY + 52 + pRandom.Next(-12, 12), pBuilder, this, pPopulace, false);
                person.Job = Job.Builder;
                pPopulace.People.Add(person);
            }

            #endregion
            #region Buildings

            bool validPosition;
            for (index = 0; index < pRandom.Next(40); index++)
            {
                validPosition = true;
                Point pos = Person.GetRandomPoint(this, pRandom, TinyReligion.TreeRadius / 3);
                foreach(Building building in Buildings)
                    if (Person.Distance(new Vector2(building.X,building.Y), new Vector2(pos.X,pos.Y)) < building.Radius / 5 + TinyReligion.TreeRadius / 5)
                        validPosition = false;
                if (validPosition)
                {
                    Buildings.Add(new Building(pos.X, pos.Y, TinyReligion.TreeRadius, pRandom.Next(TinyReligion.TreeGrowthTime), BuildingType.Tree, pTree));
                }
            }
            for (index = 0; index < pRandom.Next(10); index++)
            {
                validPosition = true;
                Point pos = Person.GetRandomPoint(this, pRandom, TinyReligion.HouseRadius / 3);
                foreach (Building building in Buildings)
                    if (Person.Distance(new Vector2(building.X, building.Y), new Vector2(pos.X, pos.Y)) < building.Radius / 5 + TinyReligion.HouseRadius / 5)
                        validPosition = false;
                if (validPosition)
                {
                    Buildings.Add(new Building(pos.X, pos.Y, TinyReligion.HouseRadius, pRandom.Next(TinyReligion.HouseConstructionTime), BuildingType.House, pHouse));
                }
            }
            for (index = 0; index < pRandom.Next(2); index++)
            {
                validPosition = true;
                Point pos = Person.GetRandomPoint(this, pRandom, TinyReligion.BarracksRadius / 3);
                foreach (Building building in Buildings)
                    if (Person.Distance(new Vector2(building.X, building.Y), new Vector2(pos.X, pos.Y)) < building.Radius / 5 + TinyReligion.BarracksRadius / 5)
                        validPosition = false;
                if (validPosition)
                {
                    Buildings.Add(new Building(pos.X, pos.Y, TinyReligion.BarracksRadius, pRandom.Next(TinyReligion.BarracksConstructionTime), BuildingType.Barracks, pBarracks));
                }
            }
            for (index = 0; index < pRandom.Next(2); index++)
            {
                validPosition = true;
                Point pos = Person.GetRandomPoint(this, pRandom, TinyReligion.TempleRadius / 3);
                foreach (Building building in Buildings)
                    if (Person.Distance(new Vector2(building.X, building.Y), new Vector2(pos.X, pos.Y)) < building.Radius / 5 + TinyReligion.TempleRadius / 5)
                        validPosition = false;
                if (validPosition)
                {
                    Buildings.Add(new Building(pos.X, pos.Y, TinyReligion.TempleRadius, pRandom.Next(TinyReligion.TempleConstructionTime), BuildingType.Temple, pTemple));
                }
            }
            for (index = 0; index < pRandom.Next(2); index++)
            {
                validPosition = true;
                Point pos = Person.GetRandomPoint(this, pRandom, TinyReligion.WorkshopRadius / 3);
                foreach (Building building in Buildings)
                    if (Person.Distance(new Vector2(building.X, building.Y), new Vector2(pos.X, pos.Y)) < building.Radius / 5 + TinyReligion.WorkshopRadius / 5)
                        validPosition = false;
                if (validPosition)
                {
                    Buildings.Add(new Building(pos.X, pos.Y, TinyReligion.WorkshopRadius, pRandom.Next(TinyReligion.WorkshopConstructionTime), BuildingType.Workshop, pWorkshop));
                }
            }

            #endregion
        }

        public void Update(List<Person> pPopulace, int pWorkshopCount)
        {
            List<Building> ToRemove = new List<Building>();
            foreach (Building building in Buildings)
            {
                building.Update(pPopulace, pWorkshopCount);
                if (building.Type == BuildingType.FallingTree && building.ConstructionLevel <= 0)
                    ToRemove.Add(building);
            }
            foreach (Building building in ToRemove)
                Buildings.Remove(building);
        }
        public void Draw(SpriteBatch pSpriteBatch, Color pColor, int pX, object pTint, bool pPlayerOwned)
        {
            Rectangle source = new Rectangle(0, 0, Texture.Width/4, Texture.Height);
            source.X = (int)Math.Max(0, Math.Min(3, (Buildings.Count - (TreeCount * 1.5)) / 3)) * source.Width;
            Color color = Color.White;
            if (pTint != null && pTint.GetType() == color.GetType())
                color = (Color)pTint;
            pSpriteBatch.Draw(Texture, new Vector2(X - pX, Y), source, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
            source.X = 0;
            pSpriteBatch.Draw(Clouds, new Vector2(X - pX, Y), source, pColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0.01f);
            foreach (Building building in Buildings)
                building.Draw(pSpriteBatch, pX, pPlayerOwned);
        }
    }
}
