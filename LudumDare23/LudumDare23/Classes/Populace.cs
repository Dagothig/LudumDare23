using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare23.Classes
{
    class Populace
    {
        public int Persons
        {
            get
            {
                int count = 0;
                foreach (Person person in People)
                    if (!person.Summon)
                        count++;
                return count;
            }
        }
        public int Summons
        {
            get
            {
                int count = 0;
                foreach (Person person in People)
                    if (person.Summon)
                        count++;
                return count;
            }
        }
        public Color Color;
        public List<Person> People = new List<Person>();
        public List<Island> OwnedIslands = new List<Island>();

        public Populace(Color pColor)
        {
            Color = pColor;
        }
        public void Update(List<Island> pAllIslands, Random pRandom)
        {
            foreach (Person person in People)
                person.Update(pAllIslands, OwnedIslands, pRandom);
        }
        public void Draw(SpriteBatch pSpriteBatch, int pX)
        {
            foreach (Person person in People)
                person.Draw(pSpriteBatch, pX);
        }
        public List<Building> Buildings(BuildingType pType)
        {
            List<Building> buildings = new List<Building>();
            foreach (Island island in OwnedIslands)
                foreach (Building building in island.Buildings)
                    if (building.Type == pType && building.ConstructionLevel <= 0)
                        buildings.Add(building);
            return buildings;
        }
        public List<Person> Jobs(Job pJob)
        {
            List<Person> people = new List<Person>();
            foreach (Person person in People)
                if (person.Job == pJob)
                    people.Add(person);
            return people;
        }
    }
}
