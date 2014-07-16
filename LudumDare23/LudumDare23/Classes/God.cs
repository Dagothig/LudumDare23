using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare23.Classes
{
    class God
    {
        Texture2D Head, Eye, Brow, Mouth;
        float LeftEyeX = 178 + 160, LeftEyeY = 72, RightEyeX = 318 + 160, RightEyeY = 72, EyeSize = 3;

        public float LikesLife = 0;
        public float LikesAttention = 0;
        public float LikesManMade = 0;

        public float Mood = 0, OverAllMood = 0;
        public const int PreferenceModifier = 200;

        public God(Texture2D pHead, Texture2D pEye, Texture2D pBrow, Texture2D pMouth)
        {
            Head = pHead;
            Eye = pEye;
            Brow = pBrow;
            Mouth = pMouth;
        }

        public void Draw(int pPointX, int pPointY, int pX, SpriteBatch pSpriteBatch, Color pColor)
        {
            float mood = Math.Min(3,Math.Max(-3,(Mood)/ 75));
            pSpriteBatch.Draw(Head, new Vector2(160, 0), pColor);
            float proportion = EyeSize / (float)(Math.Sqrt(Math.Pow((pPointX - pX) - LeftEyeX, 2) + Math.Pow(pPointY - LeftEyeY, 2)));
            pSpriteBatch.Draw(Eye, new Vector2(((pPointX - pX) - LeftEyeX) * proportion + LeftEyeX, (pPointY - LeftEyeY) * proportion + LeftEyeY), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
            proportion = EyeSize / (float)(Math.Sqrt(Math.Pow((pPointX - pX) - RightEyeX, 2) + Math.Pow(pPointY - RightEyeY, 2)));
            pSpriteBatch.Draw(Eye, new Vector2(((pPointX - pX) - RightEyeX) * proportion + RightEyeX, (pPointY - RightEyeY) * proportion + RightEyeY), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.9f);
            Rectangle source = new Rectangle(0, Brow.Height - ((int)mood + 4) * Brow.Height/7, Brow.Width, Brow.Height / 7);
            pSpriteBatch.Draw(Brow, new Vector2(LeftEyeX - 19, LeftEyeY - 36), source, pColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0.91f);
            pSpriteBatch.Draw(Brow, new Vector2(RightEyeX - 11, RightEyeY - 38), source, pColor, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0.91f);
            source.Width = Mouth.Width;
            source.Height = Mouth.Height / 7;
            source.Y = Mouth.Height - ((int)mood + 4) * Mouth.Height / 7;
            pSpriteBatch.Draw(Mouth, new Vector2(360, 85), source, pColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0.91f);
        }
        public void Update()
        {
            OverAllMood += Mood;
            Mood /= 1.01f;
        }

        public void TrainedWarrior() 
        {
            Mood -= LikesLife;
        }
        public void TrainedPriest()
        {
            Mood += LikesAttention;
        }
        public void TrainedBuilder()
        {
            Mood += LikesManMade;
        }
        public void UntrainedWarrior()
        {
            Mood += LikesLife;
        }
        public void UntrainedPriest()
        {
            Mood -= LikesAttention;
        }
        public void UntrainedBuilder()
        {
            Mood -= LikesManMade;
        }
        public void BabiesMade(int pAmount) 
        {
            Mood += LikesLife * pAmount;
        }
        public void FinishedBridge() 
        {
            Mood += LikesManMade * 4;
        }
        public void FinishedHouse() 
        {
            Mood += LikesLife * 1.5f;
            Mood += LikesManMade * 2;
        }
        public void FinishedBarracks() 
        {
            Mood -= LikesLife * 2;
            Mood += LikesManMade * 3;
        }
        public void FinishedTemple()
        {
            Mood += LikesAttention * 2;
            Mood += LikesManMade * 3;
        }
        public void FinishedWorkshop()
        {
            Mood += LikesManMade * 4;
        }
        public void FinishedGreenHouse()
        {
            Mood -= LikesManMade * 3;
            Mood += LikesLife * 2;
        }
        public void FinishedTree() 
        {
            Mood += LikesLife;
            Mood -= LikesManMade;
        }
        public void FinishedCuttingTree()
        {
            Mood -= LikesLife;
            Mood += LikesManMade;
        }
        public void Sacrificed() 
        {
            Mood -= LikesLife * 2;
            Mood += LikesAttention;
        }
        public void Fighting() 
        {
            Mood -= LikesLife / 150;
        }
        public void Killed() 
        {
            Mood -= LikesLife * 2;
        }
        public void Converting()
        {
            Mood += LikesAttention / 150;
        }
        public void Converted()
        {
            Mood += LikesAttention;
            Mood += LikesLife;
        }
        public void Prayed()
        {
            Mood += LikesAttention;
        }
        public void Summoned(int pAmount)
        {
            Mood += (LikesAttention / 2) * pAmount;
            Mood -= (LikesLife / 2) * pAmount;
        }
    }
}
