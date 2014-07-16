using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LudumDare23.Classes
{
    public enum Job { Builder, Villager, Warrior, Priest };
    class Person
    {
        public float X, Y, Health = 100, Speed = 0.25f, animationStep = 0, animationID = 4, lastAnimSwitch = 6;
        public int SinceLastBaby, SinceLastSummon, SinceTookDamage = 0, Praying;
        public Point MovementTarget;
        public Queue<Point> Movements = new Queue<Point>();
        public Job Job = Job.Villager;
        public Texture2D Texture;
        public Island CurrentIsland;
        public Populace People;
        public bool Summon;
        public Building Building;

        public const int Width = 8, Height = 12, originX = 4, originY = 10;

        public Person(int pX, int pY, Texture2D pTexture, Island pStartingIsland, Populace pPeople, bool pSummon)
        {
            X = pX;
            Y = pY;
            Texture = pTexture;
            CurrentIsland = pStartingIsland;
            People = pPeople;
            Summon = pSummon;
        }

        public void Update(List<Island> pAllIslands, List<Island> pOwnedIslands, Random pRandom)
        {
            if (Health < 100 && pRandom.Next(100) > 35)
                Health++;
            if (SinceLastBaby < 10000)
            {
                if (Job == Job.Villager)
                    SinceLastBaby++;
                else
                    SinceLastBaby = 0;
            }
            if (SinceLastSummon < 10000)
            {
                if (Job == Job.Priest)
                    SinceLastSummon++;
                else
                    SinceLastSummon = 0;
            }
            if (SinceTookDamage < 100)
            {
                SinceTookDamage++;
            }
            if (Praying > 0)
            {
                Praying--;
            }
            // Adjust the current island
            Vector2 pos = new Vector2(X, Y);
            float distance = Distance(pos, new Vector2(CurrentIsland.ZoneX, CurrentIsland.ZoneY)), distance2;
            foreach (Island island in pAllIslands)
            {
                distance2 = Distance(pos, new Vector2(island.ZoneX, island.ZoneY));
                if (distance2 < distance)
                {
                    distance = distance2;
                    CurrentIsland = island;
                }
            }
            lastAnimSwitch--;
            if (lastAnimSwitch == 0)
            {
                animationStep = (animationStep + 1) % 3;
                lastAnimSwitch = 6;
            }
            if (MovementTarget.X == X && MovementTarget.Y == Y || MovementTarget.X == 0 || MovementTarget.Y == 0)
            {
                MovementTarget = Point.Zero;
                if (Movements.Count > 0)
                {
                    Speed = (float)pRandom.Next(25, 50) / 100f + (float)pOwnedIslands.Count/10f;
                    MovementTarget = Movements.Dequeue();
                }
                else
                {
                    if (Job == Job.Builder && Building != null && Building.ConstructionLevel > 0)
                    {
                        MoveTo(pAllIslands, pAllIslands.FirstOrDefault(t => t.Buildings.Contains(Building)), pRandom);
                        if (Building.Type == BuildingType.Bridge)
                            Movements.Enqueue(new Point(Building.X - 75 + pRandom.Next(-10, 10), Building.Y + pRandom.Next(-10, 10)));
                        else
                            Movements.Enqueue(new Point(Building.X + pRandom.Next(-10, 10), Building.Y + pRandom.Next(-10, 10)));
                    }
                    else
                    {
                        // Determine if the person wants to leave the island
                        int wantsToLeave = pRandom.Next(pAllIslands.Count + 10);
                        if (wantsToLeave < pAllIslands.Count)
                            MoveTo(pAllIslands, pAllIslands[wantsToLeave], pRandom);
                        else
                            Movements.Enqueue(GetRandomPoint(CurrentIsland, pRandom, 0));
                    }
                }
            }
            else
            {
                if (Praying > 0)
                {
                    animationID = 0;
                    return;
                }
                float distX = MovementTarget.X - X, distY = MovementTarget.Y - Y;
                float proportion = Speed / (float)Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));
                if (proportion >= 1)
                {
                    X = MovementTarget.X;
                    Y = MovementTarget.Y;
                }
                else
                {
                    X += distX * proportion;
                    Y += distY * proportion;
                }
                // Determine which animation to use
                if (distX >= 0 && Math.Abs(distX) >= Math.Abs(distY))
                    animationID = 1;
                else if (distX < 0 && Math.Abs(distX) >= Math.Abs(distY))
                    animationID = 2;
                else if (distY >= 0 && Math.Abs(distY) > Math.Abs(distX))
                    animationID = 3;
                else if (distY < 0 && Math.Abs(distY) > Math.Abs(distX))
                    animationID = 4;
            }
        }
        public void MoveTo(List<Island> pAllIslands, Island pIsland, Random pRandom)
        {
            int target = pAllIslands.IndexOf(pIsland), current = pAllIslands.IndexOf(CurrentIsland);
            int islandsDistance = target - current;
            Building bridge;
            if (islandsDistance > 0)
                for (; current < target; current++)
                {
                    bridge = pAllIslands[current].Buildings.FirstOrDefault(t => t.Type == BuildingType.Bridge);
                    if (bridge != null && bridge.ConstructionLevel <= 0)
                    {
                        Movements.Enqueue(new Point(bridge.X - 140, bridge.Y));
                        Movements.Enqueue(new Point(bridge.X + 140, bridge.Y));
                    }
                    else
                        return;
                }
            else if (islandsDistance < 0 && current > 0)
                for (; current > target; current--)
                {
                    bridge = pAllIslands[current - 1].Buildings.FirstOrDefault(t => t.Type == BuildingType.Bridge);
                    if (bridge != null && bridge.ConstructionLevel <= 0)
                    {
                        Movements.Enqueue(new Point(bridge.X + 140, bridge.Y));
                        Movements.Enqueue(new Point(bridge.X - 140, bridge.Y));
                    }
                    else
                        return;
                }
            else
                return;
        }
        public void Draw(SpriteBatch pSpriteBatch, int pX)
        {
            Color color = People.Color;
            if (SinceTookDamage < 4)
                color = Color.White;
            pSpriteBatch.Draw(Texture, new Vector2((int)X - pX, (int)Y), new Rectangle((int)animationStep * Width, (int)animationID * Height, Width, Height), color, 0, new Vector2(originX, originY), 1, SpriteEffects.None, (Y / 480f));
        }
        public static Point GetRandomPoint(Island pIsland, Random pRandom, float pRadius)
        {
            int x = pRandom.Next(pIsland.ZoneX - (int)(pIsland.ZoneWidth - pRadius) / 2, pIsland.ZoneX + (int)(pIsland.ZoneWidth - pRadius) / 2);
            int max = (int)(Math.Sqrt((Math.Pow((pIsland.ZoneHeight - pRadius) / 2, 2) * (1 - (Math.Pow((x - pIsland.ZoneX), 2) / Math.Pow((pIsland.ZoneWidth - pRadius) / 2, 2))))));
            int y = pRandom.Next(pIsland.ZoneY - max, pIsland.ZoneY + max);
            return new Point(x, y);
        }
        public static float Distance(Vector2 p1, Vector2 p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
