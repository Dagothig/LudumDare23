using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
// === 1428 === //
namespace LudumDare23
{
    public enum Note { DO = 0, RE = 1, MI = 2, FA = 3, SOL = 4, LA = 5, SI = 6 };
    public enum NoteModifier { FLAT = -1, NORMAL = 0, SHARP = 1 };
    public static class Methods
    {
        public static void Write(string pText, int pX, int pY, float pDepth, Color pColor, Texture2D pFont, SpriteBatch pSpriteBatch)
        {
            int charWidth = pFont.Width / 6, charHeight = pFont.Height / 7, charNum;
            Rectangle dest = new Rectangle(pX, pY, charWidth, charHeight),
                      source = new Rectangle(0, 0, charWidth, charHeight);
            for (int index = 0; index < pText.Length; index++)
            {
                // determine the character number
                charNum = (int)pText[index];
                if (charNum >= 97 && charNum < 123)
                    charNum -= 97;
                else if (charNum >= 48 && charNum < 58)
                    charNum -= 22;
                else if (charNum == (int)'.')
                    charNum = 36;
                else if (charNum == (int)',')
                    charNum = 37;
                else if (charNum == 39)
                    charNum = 38;
                else if (charNum == (int)'!')
                    charNum = 39;
                else if (charNum == (int)'?')
                    charNum = 40;
                else if (charNum == (int)'_')
                    charNum = 41;
                else
                    charNum = -1;
                source.X = (charNum % 6) * charWidth;
                source.Y = (charNum / 6) * charHeight;
                if (charNum != -1)
                    pSpriteBatch.Draw(pFont, dest, source, pColor, 0, Vector2.Zero, SpriteEffects.None, pDepth);
                dest.X += charWidth;
            }
        }
        public static void PlayNote(SoundEffect[] pSound, int pOctave, Note pNote, NoteModifier pNoteModifier)
        {
            float note;
            switch (pNote)
            {
                case Note.DO:
                    note = 0;
                    break;
                case Note.RE:
                    note = 2;
                    break;
                case Note.MI:
                    note = 4;
                    break;
                case Note.FA:
                    note = 5;
                    break;
                case Note.SOL:
                    note = 7;
                    break;
                case Note.LA:
                    note = 9;
                    break;
                case Note.SI:
                    note = 11;
                    break;
                default:
                    note = 0;
                    break;
            }
            note += (int)pNoteModifier;
            pSound[Math.Max(0, Math.Min(4, pOctave))].Play(0.50f, note / 12, 0);
        }
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TinyReligion : Microsoft.Xna.Framework.Game
    {
        #region Variables

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D TitleScreen, Slider, Win, Loss, Font, BackGround, GodHead, Eye, Brow, Mouth, Island, Clouds, Bridge, House, Barracks, Temple, Workshop, GreenHouse, Tree, FallingTree, Villager, Warrior, Builder, Priest, White, SpecialEffects, Lightning,
            Boutton, BouttonPause, BouttonMenu, BouttonExit, BouttonTiny, BouttonShort, BouttonMedium, BouttonLong, BouttonMusic, BouttonSound, BouttonDo,
            BouttonDeforestation, BouttonForestation, BouttonHouse, BouttonBarracks, BouttonBridge, BouttonGreenhouse, BouttonWorkshop, BouttonTemple,
            BouttonWarrior, BouttonBuilder, BouttonPriest, BouttonBabies, BouttonSummon;
        SoundEffect TitleSound, WinSound, LossSound, NewSound, Babies, WarriorTrain, PriestTrain, VillagerTrain, BuilderTrain, Hit1, Hit2, Hit3, Kill, Convert, IslandWin, IslandLose, BuildingDone, BuildingStarted, Summon, Pray;
        SoundEffect[] NoteType1 = new SoundEffect[5];
        Color Sky = new Color(150, 160, 170), DarkSky = new Color(180, 30, 15), GoodSky = new Color(100, 190, 255), GodColor = new Color(255, 255, 255);
        Color CloudsColor = new Color(140, 160, 180), DarkClouds = new Color(120, 60, 80), GoodClouds = new Color(150, 220, 255);
        Classes.Populace PlayerPop, EnemyPop;
        Point GodPoint;
        Random Random = new Random();
        List<Classes.Island> Islands = new List<Classes.Island>();
        List<Classes.SpecialEffect> SFX = new List<Classes.SpecialEffect>();
        Classes.Music Music, CombatMusic;
        long SinceStart = 0;
        Classes.God God;
        int Flash = 255, SinceTree = 0, SinceMood = MoodLength, SinceAIMove, CursorPressPos = 0, Selection = 0;
        MouseState Curseur;
        int maxPeople
        {
            get
            { return 5 * PlayerPop.OwnedIslands.Count + 5 * PlayerPop.Buildings(Classes.BuildingType.House).Count + (PlayerPop.Buildings(Classes.BuildingType.Tree).Count / 4); }
        }
        int maxBuildings
        {
            get
            { return PlayerPop.OwnedIslands.Count / 2 + 1; }
        }
        int maxEnemyPeople
        {
            get
            { return 5 * EnemyPop.OwnedIslands.Count + 5 * EnemyPop.Buildings(Classes.BuildingType.House).Count + (EnemyPop.Buildings(Classes.BuildingType.Tree).Count / 4); }
        }
        int maxSummon
        {
            get
            { return 10 * PlayerPop.Buildings(Classes.BuildingType.Temple).Count; }
        }
        List<Classes.Building> BuildingsInProgress = new List<Classes.Building>();
        bool OnTitleScreen = true, SoundOn = true, JustBootUp = true, Pause = false, Gauche = false;
        bool BuildingInProgress
        {
            get { return BuildingsInProgress.Count >= maxBuildings; }
        }
        public float Goal = ShortGoal;

        Classes.Button btnPause, btnMenu, btnDo, btnBuild, btnTrain, btnUntrain,
                       btnExit, btnNewTiny, btnNewShort, btnNewMedium, btnNewLong, btnMusic, btnSound,
                       btnForestation, btnDeforestation, btnSacrifice, btnAttack, btnConvert, btnRetreat, btnPray, btnBabies, btnSummon,
                       btnHouse, btnBarracks, btnTemple, btnWorkshop, btnGreenhouse, btnBridge,
                       btnTrainWarrior, btnTrainBuilder, btnTrainPriest,
                       btnUntrainWarrior, btnUntrainBuilder, btnUntrainPriest,
                       CurrentButton = null;

        #endregion
        #region Constants

        public const float TinyGoal = 3000000, ShortGoal = 6000000, MediumGoal = 12000000, LongGoal = 24000000;
        public const int MoodLength = 5000;
        public const int TreeRadius = 40, TreeGrowthTime = 1000;
        public const int FallingTreeRadius = 40, TreeCutTime = 250;
        public const int HouseRadius = 90, HouseConstructionTime = 2000;
        public const int BarracksRadius = 150, BarracksConstructionTime = 10000;
        public const int TempleRadius = 150, TempleConstructionTime = 10000;
        public const int WorkshopRadius = 150, WorkshopConstructionTime = 10000;
        public const int GreenHouseRadius = 150, GreenHouseConstructionTime = 10000;
        public const int BridgeRadius = 360, BridgeConstructionTime = 10000;
        public const int DoBabies = 3000;
        public const int DoSummon = 5000;
        public const int DoTree = 250;

        #endregion

        public TinyReligion()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 840;
            graphics.PreferredBackBufferHeight = 680;
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            #region Textures

            TitleScreen = Content.Load<Texture2D>("Graphics/TitleScreen");
            Win = Content.Load<Texture2D>("Graphics/GodHappy");
            Loss = Content.Load<Texture2D>("Graphics/GodAngry");
            BackGround = Content.Load<Texture2D>("Graphics/BackGround800");
            Island = Content.Load<Texture2D>("Graphics/Island");
            Clouds = Content.Load<Texture2D>("Graphics/Cloud");
            Font = Content.Load<Texture2D>("Graphics/Font");
            GodHead = Content.Load<Texture2D>("Graphics/God");
            Eye = Content.Load<Texture2D>("Graphics/Eye");
            Brow = Content.Load<Texture2D>("Graphics/Brow");
            Mouth = Content.Load<Texture2D>("Graphics/Mouth");
            Bridge = Content.Load<Texture2D>("Graphics/Bridge");
            House = Content.Load<Texture2D>("Graphics/House");
            Barracks = Content.Load<Texture2D>("Graphics/Barracks");
            Temple = Content.Load<Texture2D>("Graphics/Temple");
            Workshop = Content.Load<Texture2D>("Graphics/Workshop");
            GreenHouse = Content.Load<Texture2D>("Graphics/GreenHouse");
            Tree = Content.Load<Texture2D>("Graphics/Tree");
            FallingTree = Content.Load<Texture2D>("Graphics/FallingTree");
            Villager = Content.Load<Texture2D>("Graphics/Villager");
            Priest = Content.Load<Texture2D>("Graphics/Priest");
            Warrior = Content.Load<Texture2D>("Graphics/Warrior");
            Builder = Content.Load<Texture2D>("Graphics/Builder");
            White = Content.Load<Texture2D>("Graphics/PointBlanc");
            SpecialEffects = Content.Load<Texture2D>("Graphics/SpecialEffects");
            Lightning = Content.Load<Texture2D>("Graphics/Lightning");
            Slider = Content.Load<Texture2D>("Graphics/Slider");
            #region bouttons
            Boutton = Content.Load<Texture2D>("Graphics/Boutton");
            BouttonPause = Content.Load<Texture2D>("Graphics/BouttonPause");
            BouttonMenu = Content.Load<Texture2D>("Graphics/BouttonMenu");
            BouttonExit = Content.Load<Texture2D>("Graphics/BouttonExit");
            BouttonTiny = Content.Load<Texture2D>("Graphics/BouttonTiny");
            BouttonShort = Content.Load<Texture2D>("Graphics/BouttonShort");
            BouttonMedium = Content.Load<Texture2D>("Graphics/BouttonMedium");
            BouttonLong = Content.Load<Texture2D>("Graphics/BouttonLong");
            BouttonMusic = Content.Load<Texture2D>("Graphics/BouttonMusic");
            BouttonSound = Content.Load<Texture2D>("Graphics/BouttonSound");
            BouttonDo = Content.Load<Texture2D>("Graphics/BouttonDo");
            BouttonBridge = Content.Load<Texture2D>("Graphics/BouttonBridge");
            BouttonHouse = Content.Load<Texture2D>("Graphics/BouttonHouse");
            BouttonBarracks = Content.Load<Texture2D>("Graphics/BouttonBarracks");
            BouttonForestation = Content.Load<Texture2D>("Graphics/BouttonForestation");
            BouttonDeforestation = Content.Load<Texture2D>("Graphics/BouttonDeforestation");
            BouttonGreenhouse = Content.Load<Texture2D>("Graphics/BouttonGreenhouse");
            BouttonTemple = Content.Load<Texture2D>("Graphics/BouttonTemple");
            BouttonWorkshop = Content.Load<Texture2D>("Graphics/BouttonWorkshop");
            BouttonWarrior = Content.Load<Texture2D>("Graphics/BouttonWarrior");
            BouttonBuilder = Content.Load<Texture2D>("Graphics/BouttonBuilder");
            BouttonPriest = Content.Load<Texture2D>("Graphics/BouttonPriest");
            BouttonBabies = Content.Load<Texture2D>("Graphics/BouttonBabies");
            BouttonSummon = Content.Load<Texture2D>("Graphics/BouttonSummon");
            #endregion

            #endregion
            #region Bouttons

            btnPause = new Classes.Button(8, 16, 48, 48, BouttonPause);
            btnPause.Click += new EventHandler(Click);

            btnMenu = new Classes.Button(8, 80, 48, 48, BouttonMenu);
            btnMenu.Click += new EventHandler(Click);
            #region Menu

            btnExit = new Classes.Button(8 + 64 * 1, 80, 48, 48, BouttonExit);
            btnExit.Click += new EventHandler(Click);
            btnNewTiny = new Classes.Button(8 + 64 * 2, 80, 48, 48, BouttonTiny);
            btnNewTiny.Click += new EventHandler(Click);
            btnNewShort = new Classes.Button(8 + 64 * 3, 80, 48, 48, BouttonShort);
            btnNewShort.Click += new EventHandler(Click);
            btnNewMedium = new Classes.Button(8 + 64 * 4, 80, 48, 48, BouttonMedium);
            btnNewMedium.Click += new EventHandler(Click);
            btnNewLong = new Classes.Button(8 + 64 * 5, 80, 48, 48, BouttonLong);
            btnNewLong.Click += new EventHandler(Click);
            btnMusic = new Classes.Button(8 + 64 * 6, 80, 48, 48, BouttonMusic);
            btnMusic.Click += new EventHandler(Click);
            btnSound = new Classes.Button(8 + 64 * 7, 80, 48, 48, BouttonSound);
            btnSound.Click += new EventHandler(Click);

            #endregion

            btnDo = new Classes.Button(8, 480 - 64 * 4, 48, 48, BouttonDo);
            btnDo.Click += new EventHandler(Click);
            #region Do

            btnForestation = new Classes.Button(8 + 64 * 1, 480 - 64 * 4, 48, 48, BouttonForestation);
            btnForestation.Click += new EventHandler(Click);
            btnDeforestation = new Classes.Button(8 + 64 * 2, 480 - 64 * 4, 48, 48, BouttonDeforestation);
            btnDeforestation.Click += new EventHandler(Click);
            btnSacrifice = new Classes.Button(8 + 64 * 3, 480 - 64 * 4, 48, 48, Boutton);
            btnSacrifice.Click += new EventHandler(Click);
            btnAttack = new Classes.Button(8 + 64 * 4, 480 - 64 * 4, 48, 48, Boutton);
            btnAttack.Click += new EventHandler(Click);
            btnConvert = new Classes.Button(8 + 64 * 5, 480 - 64 * 4, 48, 48, Boutton);
            btnConvert.Click += new EventHandler(Click);
            btnRetreat = new Classes.Button(8 + 64 * 6, 480 - 64 * 4, 48, 48, Boutton);
            btnRetreat.Click += new EventHandler(Click);
            btnPray = new Classes.Button(8 + 64 * 7, 480 - 64 * 4, 48, 48, Boutton);
            btnPray.Click += new EventHandler(Click);
            btnBabies = new Classes.Button(8 + 64 * 8, 480 - 64 * 4, 48, 48, BouttonBabies);
            btnBabies.Click += new EventHandler(Click);
            btnSummon = new Classes.Button(8 + 64 * 9, 480 - 64 * 4, 48, 48, BouttonSummon);
            btnSummon.Click += new EventHandler(Click);

            #endregion

            btnBuild = new Classes.Button(8, 480 - 64 * 3, 48, 48, Boutton);
            btnBuild.Click += new EventHandler(Click);
            #region Build

            btnHouse = new Classes.Button(8 + 64 * 1, 480 - 64 * 3, 48, 48, BouttonHouse);
            btnHouse.Click += new EventHandler(Click);
            btnBarracks = new Classes.Button(8 + 64 * 2, 480 - 64 * 3, 48, 48, BouttonBarracks);
            btnBarracks.Click += new EventHandler(Click);
            btnTemple = new Classes.Button(8 + 64 * 3, 480 - 64 * 3, 48, 48, BouttonTemple);
            btnTemple.Click += new EventHandler(Click);
            btnWorkshop = new Classes.Button(8 + 64 * 4, 480 - 64 * 3, 48, 48, BouttonWorkshop);
            btnWorkshop.Click += new EventHandler(Click);
            btnGreenhouse = new Classes.Button(8 + 64 * 5, 480 - 64 * 3, 48, 48, BouttonGreenhouse);
            btnGreenhouse.Click += new EventHandler(Click);
            btnBridge = new Classes.Button(8 + 64 * 6, 480 - 64 * 3, 48, 48, BouttonBridge);
            btnBridge.Click += new EventHandler(Click);

            #endregion

            btnTrain = new Classes.Button(8, 480 - 64 * 2, 48, 48, Boutton);
            btnTrain.Click += new EventHandler(Click);
            #region Train

            btnTrainWarrior = new Classes.Button(8 + 64 * 1, 480 - 64 * 2, 48, 48, BouttonWarrior);
            btnTrainWarrior.Click += new EventHandler(Click);
            btnTrainBuilder = new Classes.Button(8 + 64 * 2, 480 - 64 * 2, 48, 48, BouttonBuilder);
            btnTrainBuilder.Click += new EventHandler(Click);
            btnTrainPriest = new Classes.Button(8 + 64 * 3, 480 - 64 * 2, 48, 48, BouttonPriest);
            btnTrainPriest.Click += new EventHandler(Click);
            
            #endregion

            btnUntrain = new Classes.Button(8, 480 - 64, 48, 48, Boutton);
            btnUntrain.Click += new EventHandler(Click);
            #region Untrain

            btnUntrainWarrior = new Classes.Button(8 + 64 * 1, 480 - 64 * 1, 48, 48, BouttonWarrior);
            btnUntrainWarrior.Click += new EventHandler(Click);
            btnUntrainBuilder = new Classes.Button(8 + 64 * 2, 480 - 64 * 1, 48, 48, BouttonBuilder);
            btnUntrainBuilder.Click += new EventHandler(Click);
            btnUntrainPriest = new Classes.Button(8 + 64 * 3, 480 - 64 * 1, 48, 48, BouttonPriest);
            btnUntrainPriest.Click += new EventHandler(Click);

            #endregion

            #endregion
            #region Sounds

            PriestTrain = Content.Load<SoundEffect>("Sounds/PriestTrain");
            VillagerTrain = Content.Load<SoundEffect>("Sounds/VillagerTrain");
            WarriorTrain = Content.Load<SoundEffect>("Sounds/WarriorTrain");
            BuilderTrain = Content.Load<SoundEffect>("Sounds/BuilderTrain");
            Hit1 = Content.Load<SoundEffect>("Sounds/Hit1");
            Hit2 = Content.Load<SoundEffect>("Sounds/Hit2");
            Hit3 = Content.Load<SoundEffect>("Sounds/Hit3");
            Kill = Content.Load<SoundEffect>("Sounds/Death");
            IslandWin = Content.Load<SoundEffect>("Sounds/IslandWin");
            IslandLose = Content.Load<SoundEffect>("Sounds/IslandLose");
            Pray = Content.Load<SoundEffect>("Sounds/Pray");
            Convert = Content.Load<SoundEffect>("Sounds/Convert");
            Summon = Content.Load<SoundEffect>("Sounds/Summon");
            Babies = Content.Load<SoundEffect>("Sounds/DoBabies");
            BuildingStarted = Content.Load<SoundEffect>("Sounds/Build");
            BuildingDone = Content.Load<SoundEffect>("Sounds/Done");
            NewSound = Content.Load<SoundEffect>("Sounds/New");
            TitleSound = Content.Load<SoundEffect>("Sounds/TitleScreen");
            if (SoundOn)
                TitleSound.Play();
            WinSound = Content.Load<SoundEffect>("Sounds/Win");
            LossSound = Content.Load<SoundEffect>("Sounds/Loss");

            #endregion
            #region Note types

            for (int index = 0; index < 5; index++)
                NoteType1[index] = Content.Load<SoundEffect>("Sounds/NoteType1_" + index);

            #endregion
            #region Music

            Music = new Classes.Music(384, 480);
            #region 1
            // === 01 000 - 015 === //
            Music.Notes[0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            Music.Notes[0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            Music.Notes[4].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 2));
            Music.Notes[8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            Music.Notes[12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            Music.Notes[12].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            #endregion
            #region 2
            // === 02 016 - 031 === //
            Music.Notes[16].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 2));
            Music.Notes[18].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            Music.Notes[22].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[24].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            Music.Notes[26].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[28].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[30].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            #endregion
            #region 3
            // === 03 032 - 047 === //
            Music.Notes[32].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            Music.Notes[32].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            Music.Notes[36].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 2));
            Music.Notes[40].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            Music.Notes[44].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            Music.Notes[44].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 2));
            #endregion
            #region 4
            // === 04 048 - 063 === //
            Music.Notes[48].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            Music.Notes[50].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[52].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            Music.Notes[54].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[56].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[58].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[60].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            #endregion
            #region 5
            // === 05 064 - 079 === //
            Music.Notes[64].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[64].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            Music.Notes[68].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[68].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            Music.Notes[70].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            Music.Notes[72].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[72].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[74].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            Music.Notes[76].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[76].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[78].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            #endregion
            #region 6
            // === 06 080 - 095 === //
            Music.Notes[80].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[80].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[82].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            Music.Notes[84].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[84].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[86].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[88].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[88].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[92].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[94].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 3));
            #endregion
            #region 7
            // === 07 096 - 111 === //
            Music.Notes[96].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[96].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[98].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[100].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[100].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[102].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            Music.Notes[104].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[104].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[108].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[108].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[110].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            #endregion
            #region 8
            // === 08 112 - 127 === //
            Music.Notes[112].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[112].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            Music.Notes[114].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[116].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[116].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[120].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[120].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 3));
            Music.Notes[124].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[124].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            #endregion
            #region 9
            // === 09 128 - 143 === //
            Music.Notes[128].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[128].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[132].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[132].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            Music.Notes[134].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[136].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[138].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[140].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[142].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            #endregion
            #region 10
            // === 10 144 - 159 === //
            Music.Notes[144].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[144].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[148].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[148].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[152].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[152].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 3));
            Music.Notes[154].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[156].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[156].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[158].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 3));
            #endregion
            #region 11
            // === 11 160 - 175 === //
            Music.Notes[160].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[160].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[162].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 3));
            Music.Notes[164].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[164].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[166].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[168].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[168].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[170].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[172].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[172].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            #endregion
            #region 12
            // === 12 176 - 191 === //
            Music.Notes[176].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[176].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[178].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[180].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[180].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[182].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 3));
            Music.Notes[184].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[186].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[188].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[190].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 3));
            #endregion
            #region 13
            // === 13 192 - 207 === //
            Music.Notes[192].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[192].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[196].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[196].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[200].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[200].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[204].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[204].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            #endregion
            #region 14
            // === 14 208 - 223 === //
            Music.Notes[208].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[208].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[210].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[212].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[212].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[214].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[216].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[216].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[218].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[220].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[220].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[222].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            #endregion
            #region 15
            // === 15 224 - 239 === //
            Music.Notes[224].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[224].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[226].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[228].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[228].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[230].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[232].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[232].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[234].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[236].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[236].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 3));
            Music.Notes[238].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            #endregion
            #region 16
            // === 16 240 - 255 === //
            Music.Notes[240].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[240].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            Music.Notes[242].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[244].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[244].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[248].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[248].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[250].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            Music.Notes[252].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[252].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[254].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            #endregion
            #region 17
            // === 17 256 - 271 === //
            Music.Notes[256].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[256].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[256].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[256].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[258].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            Music.Notes[260].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[260].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[260].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[260].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[262].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[264].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[264].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[264].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[264].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[266].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[268].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[268].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 4));
            Music.Notes[268].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[268].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[270].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            #endregion
            #region 18
            // === 18 272 - 287 === //
            Music.Notes[272].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            Music.Notes[272].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[272].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[272].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[274].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[276].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[276].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[276].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[276].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[278].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            Music.Notes[280].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[280].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[280].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            Music.Notes[280].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[282].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[284].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            Music.Notes[284].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[284].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[284].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[286].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            #endregion
            #region 19
            // === 19 288 - 303 === //
            Music.Notes[288].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[288].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[288].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[288].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[290].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[292].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[292].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 4));
            Music.Notes[292].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[292].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[294].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[296].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[296].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[296].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[296].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[298].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[300].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            Music.Notes[300].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[300].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            Music.Notes[300].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[302].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            #endregion
            #region 20
            // === 20 304 - 319 === //
            Music.Notes[304].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[304].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[304].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[304].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[306].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 1));
            Music.Notes[308].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[308].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[308].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[308].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[310].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[312].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 2));
            Music.Notes[312].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[312].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[312].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[314].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 2));
            Music.Notes[316].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            Music.Notes[316].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[316].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[316].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[318].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            #endregion
            #region 21
            // === 21 320 - 335 === //
            Music.Notes[320].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[320].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[320].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[320].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[322].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            Music.Notes[324].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[324].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[324].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[324].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[326].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            Music.Notes[328].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[328].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[328].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[328].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[330].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[332].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[332].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 4));
            Music.Notes[332].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[332].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[334].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            #endregion
            #region 22
            // === 22 336 - 351 === //
            Music.Notes[336].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            Music.Notes[336].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[336].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[336].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[338].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 1));
            Music.Notes[340].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[340].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[340].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[340].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[342].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            Music.Notes[344].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[344].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[344].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            Music.Notes[344].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[346].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[348].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            Music.Notes[348].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            Music.Notes[348].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[348].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[350].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            #endregion
            #region 23
            // === 23 352 - 367 === //
            Music.Notes[352].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[352].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[352].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[352].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[354].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 1));
            Music.Notes[356].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[356].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 4));
            Music.Notes[356].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[356].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[358].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            Music.Notes[360].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[360].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[360].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[360].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[362].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[364].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            Music.Notes[364].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[364].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            Music.Notes[364].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[366].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 1));
            #endregion
            #region 24
            // === 24 368 - 383 === //
            Music.Notes[368].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[368].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[368].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[368].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            Music.Notes[370].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.NORMAL, 1));
            Music.Notes[372].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            Music.Notes[372].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[372].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[372].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            Music.Notes[374].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            Music.Notes[376].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 2));
            Music.Notes[376].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[376].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 4));
            Music.Notes[376].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            Music.Notes[378].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.NORMAL, 2));
            Music.Notes[380].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            Music.Notes[380].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            Music.Notes[380].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            Music.Notes[380].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
            Music.Notes[382].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 1));
            #endregion

            #endregion
            #region Combat Music

            CombatMusic = new Classes.Music(40 * 16, 640);
            int mesure = 0;
            #region 1
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            mesure++;
            #endregion
            #region 2
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            mesure++;
            #endregion
            #region 3
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            mesure++;
            #endregion
            #region 4
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            mesure++;
            #endregion
            #region 5
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            mesure++;
            #endregion
            #region 6
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            mesure++;
            #endregion
            #region 7
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            mesure++;
            #endregion
            #region 8
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            mesure++;
            #endregion
            #region 9
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            mesure++;
            #endregion
            #region 10
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            mesure++;
            #endregion
            #region 11
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            mesure++;
            #endregion
            #region 12
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
            mesure++;
            #endregion
            #region 13
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 14
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 15
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 16
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 9].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 11].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 17
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 18
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 19
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 20
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 9].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 11].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 21

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 5].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 9].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 22

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 5].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 11].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
            }
            mesure++;
            #endregion
            #region 23

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 5].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 9].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 24

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 5].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 11].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 25

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 5].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 9].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 26

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 1));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 5].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 11].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
            }
            mesure++;
            #endregion
            #region 27

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 5].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 9].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 28

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 5].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 11].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 29
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 30
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 31
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 32
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 9].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 11].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 13].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 33
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 34
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 35
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
            }
            mesure++;
            #endregion
            #region 36
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 2));
            CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 14].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 2));

            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 1].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 3].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 7].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));

            for (int index = 0; index < 2; index++)
            {
                CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 4));
                CombatMusic.Notes[mesure * 16 + 2].Add(new Classes.MusicNote(NoteType1, Note.SI, NoteModifier.FLAT, 3));
                CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.LA, NoteModifier.NORMAL, 3));
                CombatMusic.Notes[mesure * 16 + 6].Add(new Classes.MusicNote(NoteType1, Note.SOL, NoteModifier.NORMAL, 3));
                CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.FA, NoteModifier.NORMAL, 3));
                CombatMusic.Notes[mesure * 16 + 10].Add(new Classes.MusicNote(NoteType1, Note.MI, NoteModifier.FLAT, 3));
                CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            }
            mesure++;
            #endregion
            #region 37
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            mesure++;
            #endregion
            #region 38
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 1));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.DO, NoteModifier.NORMAL, 2));
            mesure++;
            #endregion
            #region 39
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            mesure++;
            #endregion
            #region 40
            CombatMusic.Notes[mesure * 16 + 0].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 3));
            CombatMusic.Notes[mesure * 16 + 4].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            CombatMusic.Notes[mesure * 16 + 8].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 1));
            CombatMusic.Notes[mesure * 16 + 12].Add(new Classes.MusicNote(NoteType1, Note.RE, NoteModifier.NORMAL, 2));
            mesure++;
            #endregion

            #endregion
            NewGame(false);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            Curseur = Mouse.GetState();
            if (!OnTitleScreen)
            {
                bool gauche = Curseur.LeftButton == ButtonState.Pressed;
                if (gauche && !Gauche)
                    CursorPressPos = Curseur.X;
                if (!gauche && Gauche)
                {
                    Selection -= Curseur.X - CursorPressPos;
                    Selection = Math.Max(0, Math.Min((Islands.Count - 1) * 480 - 320, Selection));
                }
                Gauche = gauche;

                #region Bouttons

                btnPause.Update(ref Curseur);
                btnMenu.Update(ref Curseur);
                #region Menu
                if (CurrentButton == btnMenu)
                {
                    btnExit.Update(ref Curseur);
                    btnNewTiny.Update(ref Curseur);
                    btnNewShort.Update(ref Curseur);
                    btnNewMedium.Update(ref Curseur);
                    btnNewLong.Update(ref Curseur);
                    btnMusic.Update(ref Curseur);
                    btnSound.Update(ref Curseur);
                }
                #endregion
                btnDo.Update(ref Curseur);
                #region Do
                if (CurrentButton == btnDo)
                {
                    btnForestation.Update(ref Curseur);
                    btnDeforestation.Update(ref Curseur);
                    btnSacrifice.Update(ref Curseur);
                    btnAttack.Update(ref Curseur);
                    btnConvert.Update(ref Curseur);
                    btnRetreat.Update(ref Curseur);
                    btnPray.Update(ref Curseur);
                    btnBabies.Update(ref Curseur);
                    btnSummon.Update(ref Curseur);
                }
                #endregion
                btnBuild.Update(ref Curseur);
                #region Build
                if (CurrentButton == btnBuild)
                {
                    btnHouse.Update(ref Curseur);
                    btnBarracks.Update(ref Curseur);
                    btnTemple.Update(ref Curseur);
                    btnWorkshop.Update(ref Curseur);
                    btnGreenhouse.Update(ref Curseur);
                    btnBridge.Update(ref Curseur);
                }
                #endregion
                btnTrain.Update(ref Curseur);
                #region Train
                if (CurrentButton == btnTrain)
                {
                    btnTrainWarrior.Update(ref Curseur);
                    btnTrainBuilder.Update(ref Curseur);
                    btnTrainPriest.Update(ref Curseur);
                }
                #endregion
                btnUntrain.Update(ref Curseur);
                #region Untrain
                if (CurrentButton == btnUntrain)
                {
                    btnUntrainWarrior.Update(ref Curseur);
                    btnUntrainBuilder.Update(ref Curseur);
                    btnUntrainPriest.Update(ref Curseur);
                }
                #endregion

                #endregion
            }
            #region pas en pause
            if (!Pause)
            {
                Music.Update();
                CombatMusic.Update();
                if (OnTitleScreen)
                {
                    MouseState state = Mouse.GetState();
                    OnTitleScreen = !(Mouse.GetState().LeftButton == ButtonState.Pressed && state.X > 0 && state.Y < 640 && state.Y > 0 && state.Y < 480);
                    if (!OnTitleScreen)
                    {
                        NewGame(true);
                    }
                }
                else
                {
                    SinceStart++;
                    if (SinceTree > 0)
                        SinceTree--;
                    if (SinceMood > 0)
                        SinceMood--;
                    else
                    {
                        SinceMood = MoodLength;
                        Flash = 255;
                        AssignGod(false);
                        if (SoundOn)
                            NewSound.Play();
                    }
                    PlayerPop.Update(Islands, Random);
                    God.Update();
                    // If god angry, god kill villagers
                    if (God.OverAllMood < 0 && Random.Next(0, 3500) < Math.Pow(-(God.OverAllMood / (Goal / 4)), 6) * 100)
                    {
                        Classes.Person unluckyDude = PlayerPop.People[Random.Next(PlayerPop.People.Count)];
                        PlayerPop.People.Remove(unluckyDude);
                        SFX.Add(new Classes.SpecialEffect((int)unluckyDude.X, (int)unluckyDude.Y, 0, Lightning, true));
                        SFX.Add(new Classes.SpecialEffect((int)unluckyDude.X, (int)unluckyDude.Y, 0, SpecialEffects, false));
                        if (SoundOn)
                            Kill.Play();
                        Flash = 150;
                        God.Sacrificed();
                    }
                    if (EnemyPop != null)
                    {
                        if (SinceAIMove > 0)
                            SinceAIMove--;
                        else
                        {
                            SinceAIMove = 10;
                            AI();
                        }
                        if (EnemyPop.People.Count > 0 || EnemyPop.OwnedIslands.Count > 0)
                        {
                            EnemyPop.Update(Islands, Random);
                            #region ResolveConflicts

                            List<Classes.Person> playerPop = new List<Classes.Person>(), enemyPop = new List<Classes.Person>();
                            Classes.Person tmp;
                            int index;
                            foreach (Classes.Island island in Islands)
                            {
                                playerPop.Clear();
                                enemyPop.Clear();
                                // Get the people on the island
                                foreach (Classes.Person person in PlayerPop.People)
                                    if (person.CurrentIsland == island)
                                        playerPop.Add(person);
                                foreach (Classes.Person person in EnemyPop.People)
                                    if (person.CurrentIsland == island)
                                        enemyPop.Add(person);
                                // Actually resolve conflicts on the island
                                if (playerPop.Count > 0 && enemyPop.Count > 0)
                                {
                                    int attempts, r;
                                    #region Player

                                    foreach (Classes.Person person in playerPop)
                                    {
                                        if (enemyPop.Count > 0)
                                        {
                                            switch (person.Job)
                                            {
                                                #region Warrior

                                                case Classes.Job.Warrior:
                                                    attempts = 0;
                                                    while (attempts < enemyPop.Count)
                                                    {
                                                        attempts++;
                                                        index = Random.Next(enemyPop.Count);
                                                        if (Classes.Person.Distance(new Vector2(person.X, person.Y), new Vector2(enemyPop[index].X, enemyPop[index].Y)) < 48)
                                                        {
                                                            attempts = enemyPop.Count;
                                                            tmp = enemyPop[index];
                                                            tmp.Health -= 1 + PlayerPop.Buildings(Classes.BuildingType.Barracks).Count;
                                                            r = Random.Next(100);
                                                            if (SoundOn)
                                                                if (r < 1)
                                                                    Hit1.Play();
                                                                else if (r < 2)
                                                                    Hit2.Play();
                                                                else if (r < 3)
                                                                    Hit3.Play();
                                                            tmp.SinceTookDamage = 0;
                                                            God.Fighting();
                                                            if (tmp.Health <= 0)
                                                            {
                                                                enemyPop.Remove(tmp);
                                                                EnemyPop.People.Remove(tmp);
                                                                God.Killed();
                                                                SFX.Add(new Classes.SpecialEffect((int)tmp.X, (int)tmp.Y, 0, SpecialEffects, false));
                                                                if (SoundOn)
                                                                    Kill.Play();
                                                                GodPoint.X = (int)tmp.X;
                                                                GodPoint.Y = (int)tmp.Y;
                                                            }
                                                        }
                                                    }
                                                    break;

                                                #endregion
                                                #region Villager & Builder

                                                case Classes.Job.Villager:
                                                case Classes.Job.Builder:
                                                    attempts = 0;
                                                    while (attempts < enemyPop.Count)
                                                    {
                                                        attempts++;
                                                        index = Random.Next(enemyPop.Count);
                                                        if (Classes.Person.Distance(new Vector2(person.X, person.Y), new Vector2(enemyPop[index].X, enemyPop[index].Y)) < 48)
                                                        {
                                                            attempts = enemyPop.Count;
                                                            if (Random.Next(0, 100) < 25)
                                                            {
                                                                tmp = enemyPop[index];
                                                                tmp.Health -= 1 + PlayerPop.Buildings(Classes.BuildingType.Barracks).Count;
                                                                r = Random.Next(100);
                                                                if (SoundOn)
                                                                    if (r < 1)
                                                                        Hit1.Play();
                                                                    else if (r < 2)
                                                                        Hit2.Play();
                                                                    else if (r < 3)
                                                                        Hit3.Play();
                                                                tmp.SinceTookDamage = 0;
                                                                God.Fighting();
                                                                if (tmp.Health <= 0)
                                                                {
                                                                    enemyPop.Remove(tmp);
                                                                    EnemyPop.People.Remove(tmp);
                                                                    God.Killed();
                                                                    SFX.Add(new Classes.SpecialEffect((int)tmp.X, (int)tmp.Y, 0, SpecialEffects, false));
                                                                    if (SoundOn)
                                                                        Kill.Play();
                                                                    GodPoint.X = (int)tmp.X;
                                                                    GodPoint.Y = (int)tmp.Y;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;

                                                #endregion
                                                #region Priest

                                                case Classes.Job.Priest:
                                                    attempts = 0;
                                                    while (attempts < enemyPop.Count)
                                                    {
                                                        attempts++;
                                                        index = Random.Next(enemyPop.Count);
                                                        if (Classes.Person.Distance(new Vector2(person.X, person.Y), new Vector2(enemyPop[index].X, enemyPop[index].Y)) < 64)
                                                        {
                                                            attempts = enemyPop.Count;
                                                            God.Converting();
                                                            if (Random.Next(0, 1000) < PlayerPop.Buildings(Classes.BuildingType.Temple).Count + 1)
                                                            {
                                                                tmp = enemyPop[index];
                                                                tmp.People = PlayerPop;
                                                                EnemyPop.People.Remove(tmp);
                                                                PlayerPop.People.Add(tmp);
                                                                God.Converted();
                                                                SFX.Add(new Classes.SpecialEffect((int)tmp.X, (int)tmp.Y, 2, SpecialEffects, false));
                                                                if (SoundOn)
                                                                    Convert.Play();
                                                                GodPoint.X = (int)tmp.X;
                                                                GodPoint.Y = (int)tmp.Y;
                                                            }
                                                        }
                                                    }
                                                    break;

                                                #endregion
                                            }
                                        }
                                    }

                                    #endregion
                                    #region Enemy

                                    foreach (Classes.Person person in enemyPop)
                                    {
                                        if (playerPop.Count > 0)
                                        {
                                            switch (person.Job)
                                            {
                                                #region Warrior

                                                case Classes.Job.Warrior:
                                                    attempts = 0;
                                                    while (attempts < playerPop.Count)
                                                    {
                                                        attempts++;
                                                        index = Random.Next(playerPop.Count);
                                                        if (Classes.Person.Distance(new Vector2(person.X, person.Y), new Vector2(playerPop[index].X, playerPop[index].Y)) < 48)
                                                        {
                                                            attempts = playerPop.Count;
                                                            tmp = playerPop[index];
                                                            tmp.Health -= 1 + EnemyPop.Buildings(Classes.BuildingType.Barracks).Count;
                                                            r = Random.Next(100);
                                                            if (SoundOn)
                                                                if (r < 1)
                                                                    Hit1.Play();
                                                                else if (r < 2)
                                                                    Hit2.Play();
                                                                else if (r < 3)
                                                                    Hit3.Play();
                                                            tmp.SinceTookDamage = 0;
                                                            if (tmp.Health <= 0)
                                                            {
                                                                playerPop.Remove(tmp);
                                                                PlayerPop.People.Remove(tmp);
                                                                SFX.Add(new Classes.SpecialEffect((int)tmp.X, (int)tmp.Y, 0, SpecialEffects, false));
                                                                if (SoundOn)
                                                                    Kill.Play();
                                                                GodPoint.X = (int)tmp.X;
                                                                GodPoint.Y = (int)tmp.Y;
                                                            }
                                                        }
                                                    }
                                                    break;

                                                #endregion
                                                #region Villager & Builder

                                                case Classes.Job.Villager:
                                                case Classes.Job.Builder:
                                                    attempts = 0;
                                                    while (attempts < playerPop.Count)
                                                    {
                                                        attempts++;
                                                        index = Random.Next(playerPop.Count);
                                                        if (Classes.Person.Distance(new Vector2(person.X, person.Y), new Vector2(playerPop[index].X, playerPop[index].Y)) < 48)
                                                        {
                                                            attempts = playerPop.Count;
                                                            if (Random.Next(0, 100) < 25)
                                                            {
                                                                tmp = playerPop[index];
                                                                tmp.Health -= 1 + EnemyPop.Buildings(Classes.BuildingType.Barracks).Count;
                                                                r = Random.Next(100);
                                                                if (SoundOn)
                                                                    if (r < 1)
                                                                        Hit1.Play();
                                                                    else if (r < 2)
                                                                        Hit2.Play();
                                                                    else if (r < 3)
                                                                        Hit3.Play();
                                                                tmp.SinceTookDamage = 0;
                                                                if (tmp.Health <= 0)
                                                                {
                                                                    playerPop.Remove(tmp);
                                                                    PlayerPop.People.Remove(tmp);
                                                                    SFX.Add(new Classes.SpecialEffect((int)tmp.X, (int)tmp.Y, 0, SpecialEffects, false));
                                                                    if (SoundOn)
                                                                        Kill.Play();
                                                                    GodPoint.X = (int)tmp.X;
                                                                    GodPoint.Y = (int)tmp.Y;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;

                                                #endregion
                                                #region Priest

                                                case Classes.Job.Priest:
                                                    attempts = 0;
                                                    while (attempts < playerPop.Count)
                                                    {
                                                        attempts++;
                                                        index = Random.Next(playerPop.Count);
                                                        if (Classes.Person.Distance(new Vector2(person.X, person.Y), new Vector2(playerPop[index].X, playerPop[index].Y)) < 64)
                                                        {
                                                            attempts = playerPop.Count;
                                                            if (Random.Next(0, 1000) < EnemyPop.Buildings(Classes.BuildingType.Temple).Count + 1)
                                                            {
                                                                tmp = playerPop[index];
                                                                tmp.People = EnemyPop;
                                                                PlayerPop.People.Remove(tmp);
                                                                EnemyPop.People.Add(tmp);
                                                                SFX.Add(new Classes.SpecialEffect((int)tmp.X, (int)tmp.Y, 2, SpecialEffects, false));
                                                                if (SoundOn)
                                                                    Convert.Play();
                                                                GodPoint.X = (int)tmp.X;
                                                                GodPoint.Y = (int)tmp.Y;
                                                            }
                                                        }
                                                    }
                                                    break;

                                                #endregion
                                            }
                                        }
                                    }

                                    #endregion
                                }
                                if (EnemyPop.OwnedIslands.Contains(island) && enemyPop.Count == 0 && playerPop.Count > 0)
                                {
                                    PlayerPop.OwnedIslands.Add(island);
                                    EnemyPop.OwnedIslands.Remove(island);
                                    if (SoundOn)
                                        IslandWin.Play();
                                }
                                else if (PlayerPop.OwnedIslands.Contains(island) && playerPop.Count == 0 && enemyPop.Count > 0)
                                {
                                    EnemyPop.OwnedIslands.Add(island);
                                    PlayerPop.OwnedIslands.Remove(island);
                                    if (SoundOn)
                                        IslandLose.Play();
                                }
                            }

                            #endregion
                        }
                        if (EnemyPop.People.Count == 0 && EnemyPop.OwnedIslands.Count == 0)
                        {
                            EnemyPop = null;
                            if (CombatMusic.Playing)
                            {
                                CombatMusic.Stop();
                                Music.Play();
                            }
                        }
                    }
                    foreach (Classes.Island island in Islands)
                    {
                        if (PlayerPop.OwnedIslands.Contains(island))
                            island.Update(PlayerPop.People, PlayerPop.Buildings(Classes.BuildingType.Workshop).Count);
                        else if (EnemyPop != null)
                            island.Update(EnemyPop.People, EnemyPop.Buildings(Classes.BuildingType.Workshop).Count);
                    }
                    for (int index = SFX.Count - 1; index >= 0; index--)
                    {
                        if (SFX[index].IdX >= 6 && (SFX[index].IdY != 0 || SFX[index].Texture == Lightning) || SFX[index].IdX >= 60)
                            SFX.RemoveAt(index);
                        else
                            SFX[index].Update();
                    }
                    #region Keyboard

                    KeyboardState keyboard = Keyboard.GetState();

                    #endregion
                    // Game Over
                    // Win
                    if (PlayerPop.People.Count == 0)
                    {
                        GameOver(false);
                        if (CombatMusic.Playing)
                        {
                            CombatMusic.Stop();
                            Music.Play();
                        }
                    }
                    // Loss
                    if (God.OverAllMood >= Goal)
                    {
                        GameOver(true);
                        if (CombatMusic.Playing)
                        {
                            CombatMusic.Stop();
                            Music.Play();
                        }
                    }
                    // Flash reduction
                    if (Flash > 0)
                        Flash -= 10;
                }
            }
            #endregion
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            float prop;
            Color sky, clouds;
            if (God.OverAllMood < 0)
            {
                prop = Math.Min(1, Math.Max(0, -God.OverAllMood / (Goal / 4)));
                sky = new Color((byte)(DarkSky.R * prop + Sky.R * (1 - prop)), (byte)(DarkSky.G * prop + Sky.G * (1 - prop)), (byte)(DarkSky.B * prop + Sky.B * (1 - prop)));
                clouds = new Color((byte)(DarkClouds.R * prop + CloudsColor.R * (1 - prop)), (byte)(DarkClouds.G * prop + CloudsColor.G * (1 - prop)), (byte)(DarkClouds.B * prop + CloudsColor.B * (1 - prop)));
            }
            else
            {
                prop = Math.Min(1, Math.Max(0, God.OverAllMood / Goal));
                sky = new Color((byte)(GoodSky.R * prop + Sky.R * (1 - prop)), (byte)(GoodSky.G * prop + Sky.G * (1 - prop)), (byte)(GoodSky.B * prop + Sky.B * (1 - prop)));
                clouds = new Color((byte)(GoodClouds.R * prop + CloudsColor.R * (1 - prop)), (byte)(GoodClouds.G * prop + CloudsColor.G * (1 - prop)), (byte)(GoodClouds.B * prop + CloudsColor.B * (1 - prop)));
            }
            GraphicsDevice.Clear(sky);
            spriteBatch.Begin();
            spriteBatch.Draw(BackGround, Vector2.Zero, Color.White);
            spriteBatch.End();

            MouseState mouse = Mouse.GetState();
            if (OnTitleScreen)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(TitleScreen, new Rectangle(0, 0, 800, 480), new Rectangle(0, 0, 1, 1), Color.White);
                spriteBatch.Draw(TitleScreen, new Vector2(80,0), Color.White);
                if (TitleScreen == Win)
                {
                    Methods.Write("your god is super happy with you!", 230 + 80, 120, 1, Color.White, Font, spriteBatch);
                    Methods.Write("and it only took you " + (SinceStart / 3600).ToString() + " minutes and " + ((SinceStart / 60) % 60).ToString() + " seconds!", 250, 140, 1, Color.White, Font, spriteBatch);
                }
                if (TitleScreen == Loss)
                {
                    Methods.Write("your god is terribly angry at your terribad performances...", 80 + 80, 150, 1, Color.White, Font, spriteBatch);
                    Methods.Write("and it only took you " + (SinceStart / 3600).ToString() + " minutes and " + ((SinceStart / 60) % 60).ToString() + " seconds to do so", 100, 170, 1, Color.White, Font, spriteBatch);
                }
                spriteBatch.End();
            }
            else
            {
                // TODO: Add your drawing code here
                int selection;
                if (Curseur.LeftButton == ButtonState.Pressed)
                   selection = Selection - (Curseur.X - CursorPressPos);
                else
                    selection = Selection;
                selection = Math.Max(0, Math.Min((Islands.Count - 1) * 480 - 320, selection));
                // Game
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied);
                foreach (Classes.Island island in Islands)
                    island.Draw(spriteBatch, clouds, selection, null, PlayerPop.OwnedIslands.Contains(island));
                foreach (Classes.SpecialEffect sfx in SFX)
                    sfx.Draw(spriteBatch, selection);
                God.Draw(GodPoint.X, GodPoint.Y, selection, spriteBatch, GodColor);
                PlayerPop.Draw(spriteBatch, selection);
                if (EnemyPop != null)
                    EnemyPop.Draw(spriteBatch, selection);
                spriteBatch.End();
                // Flash
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive);
                if (Flash > 0)
                    spriteBatch.Draw(White, new Rectangle(0, 0, 800, 480), new Color(255, 255, 255, Flash));
                spriteBatch.End();
                // UI
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
                #region Bouttons

                btnPause.Draw(spriteBatch);
                btnMenu.Draw(spriteBatch);
                #region Menu
                if (CurrentButton == btnMenu)
                {
                    btnExit.Draw(spriteBatch);
                    btnNewTiny.Draw(spriteBatch);
                    btnNewShort.Draw(spriteBatch);
                    btnNewMedium.Draw(spriteBatch);
                    btnNewLong.Draw(spriteBatch);
                    btnMusic.Draw(spriteBatch);
                    btnSound.Draw(spriteBatch);
                }
                #endregion
                btnDo.Draw(spriteBatch);
                #region Do
                if (CurrentButton == btnDo)
                {
                    btnForestation.Draw(spriteBatch);
                    btnDeforestation.Draw(spriteBatch);
                    btnSacrifice.Draw(spriteBatch);
                    btnAttack.Draw(spriteBatch);
                    btnConvert.Draw(spriteBatch);
                    btnRetreat.Draw(spriteBatch);
                    btnPray.Draw(spriteBatch);
                    btnBabies.Draw(spriteBatch);
                    btnSummon.Draw(spriteBatch);
                }
                #endregion
                btnBuild.Draw(spriteBatch);
                #region Build
                if (CurrentButton == btnBuild)
                {
                    btnHouse.Draw(spriteBatch);
                    btnBarracks.Draw(spriteBatch);
                    btnTemple.Draw(spriteBatch);
                    btnWorkshop.Draw(spriteBatch);
                    btnGreenhouse.Draw(spriteBatch);
                    btnBridge.Draw(spriteBatch);
                }
                #endregion
                btnTrain.Draw(spriteBatch);
                #region Train
                if (CurrentButton == btnTrain)
                {
                    btnTrainWarrior.Draw(spriteBatch);
                    btnTrainBuilder.Draw(spriteBatch);
                    btnTrainPriest.Draw(spriteBatch);
                }
                #endregion
                btnUntrain.Draw(spriteBatch);
                #region Untrain
                if (CurrentButton == btnUntrain)
                {
                    btnUntrainWarrior.Draw(spriteBatch);
                    btnUntrainBuilder.Draw(spriteBatch);
                    btnUntrainPriest.Draw(spriteBatch);
                }
                #endregion

                #endregion
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        void AI()
        {
            int villagers = EnemyPop.Persons;
            List<Classes.Person> babies = new List<Classes.Person>();
            foreach (Classes.Person person in EnemyPop.People)
                if (person.Job == Classes.Job.Villager && Random.Next(0, DoBabies) < person.SinceLastBaby)
                    babies.Add(new Classes.Person((int)person.X, (int)person.Y, Villager, person.CurrentIsland, EnemyPop, false));
            foreach (Classes.Person baby in babies)
                if (EnemyPop.Persons < maxEnemyPeople)
                    EnemyPop.People.Add(baby);
            villagers = EnemyPop.Persons - villagers;
            if (villagers > 0)
            {
                foreach (Classes.Person person in EnemyPop.People)
                    person.SinceLastBaby = 0;
            }
            if (EnemyPop.Jobs(Classes.Job.Villager).Count > 10)
            {
                Classes.Person person = EnemyPop.People.FirstOrDefault(t => t.Job == Classes.Job.Villager);
                if (person != null)
                {
                    person.Job = Classes.Job.Warrior;
                    person.Texture = Warrior;
                    SFX.Add(new Classes.SpecialEffect((int)person.X, (int)person.Y, 1, SpecialEffects, false));
                }
                person = EnemyPop.People.FirstOrDefault(t => t.Job == Classes.Job.Villager);
                if (person != null)
                {
                    person.Job = Classes.Job.Priest;
                    person.Texture = Priest;
                    SFX.Add(new Classes.SpecialEffect((int)person.X, (int)person.Y, 1, SpecialEffects, false));
                }
            }
            else
            {
                Classes.Island island = PlayerPop.OwnedIslands[Random.Next(PlayerPop.OwnedIslands.Count)];
                OrderAllEnemy(Classes.Job.Warrior, island, Classes.Person.GetRandomPoint(island, Random, 0));
                island = PlayerPop.OwnedIslands[Random.Next(PlayerPop.OwnedIslands.Count)];
                OrderAllEnemy(Classes.Job.Priest, island, Classes.Person.GetRandomPoint(island, Random, 0));
            }
        }
        void NewGame(bool pPlaySound)
        {
            if (pPlaySound)
                if (SoundOn)
                {
                    NewSound.Play();
                    if (JustBootUp)
                        Music.Play();
                    JustBootUp = false;
                }
            SinceStart = 0;
            SinceMood = MoodLength/4;
            Flash = 255;
            SinceTree = 0;
            Islands.Clear();
            PlayerPop = new Classes.Populace(new Color(17, 57, 150));
            EnemyPop = null;
            Islands.Clear();
            SFX.Clear();
            BuildingsInProgress.Clear();
            God = new Classes.God(GodHead, Eye, Brow, Mouth);
            AssignGod(true);
            Islands.Add(new Classes.Island(0, 240, Island, Clouds, Tree, Random));
            Islands.Add(new Classes.Island(480, 240, Island, Clouds, Tree, Random));
            PlayerPop.OwnedIslands.Add(Islands[0]);
            PlayerPop.People.Add(new Classes.Person(250, 280, Villager, Islands[0], PlayerPop, false));
            PlayerPop.People.Add(new Classes.Person(250, 280, Villager, Islands[0], PlayerPop, false));
            PlayerPop.People.Add(new Classes.Person(250, 280, Warrior, Islands[0], PlayerPop, false));
            PlayerPop.People[2].Job = Classes.Job.Warrior;
            PlayerPop.People.Add(new Classes.Person(250, 280, Priest, Islands[0], PlayerPop, false));
            PlayerPop.People[3].Job = Classes.Job.Priest;
            PlayerPop.People.Add(new Classes.Person(250, 280, Builder, Islands[0], PlayerPop, false));
            PlayerPop.People[4].Job = Classes.Job.Builder;
            GodPoint = new Point(250, 280);
        }
        void GameOver(bool pWin)
        {
            if (pWin)
            {
                TitleScreen = Win;
                if (SoundOn)
                    WinSound.Play();
            }
            else
            {
                TitleScreen = Loss;
                if (SoundOn)
                    LossSound.Play();
            }
            OnTitleScreen = true;
        }
        void AssignGod(bool pDefault)
        {
            if (pDefault)
            {
                God.LikesLife = Classes.God.PreferenceModifier;
                God.LikesAttention = Classes.God.PreferenceModifier / 4;
                God.LikesManMade = -Classes.God.PreferenceModifier / 4;
            }
            else
            {
                God.LikesLife = Random.Next(-Classes.God.PreferenceModifier, Classes.God.PreferenceModifier);
                God.LikesAttention = Random.Next(-Classes.God.PreferenceModifier, Classes.God.PreferenceModifier);
                God.LikesManMade = Random.Next(-Classes.God.PreferenceModifier, Classes.God.PreferenceModifier);
            }
            float R = 0, G = 0, B = 0;
            if (God.LikesLife < 0)
                R -= (God.LikesLife / 200) * 255;
            else
                B += (God.LikesLife / 200) * 255;
            if (God.LikesManMade <= 0)
                G -= (God.LikesManMade / 200) * 255;
            else
            {
                if (R < 128)
                    R = 128 - ((128 - R) / ((God.LikesManMade + 50) / 50));
                else
                    R = (R - 128) / ((God.LikesManMade + 50) / 50) + 128;

                if (G < 128)
                    G = 128 - ((128 - G) / ((God.LikesManMade + 50) / 50));
                else
                    G = (G - 128) / ((God.LikesManMade + 50) / 50) + 128;

                if (B < 128)
                    B = 128 - ((128 - B) / ((God.LikesManMade + 50) / 50));
                else
                    B = (B - 128) / ((God.LikesManMade + 50) / 50) + 128;
            }
            R = R * ((float)Math.Pow(2, God.LikesAttention / 200f));
            G = G * ((float)Math.Pow(2, God.LikesAttention / 200f));
            B = B * ((float)Math.Pow(2, God.LikesAttention / 200f));
            GodColor = new Color((byte)Math.Min(R, 255), (byte)Math.Min(G, 255), (byte)Math.Min(B, 255));
        }
        void OrderAll(Classes.Job pJob, Classes.Island pIsland)
        {
            foreach (Classes.Person person in PlayerPop.Jobs(pJob))
            {
                person.Movements.Clear();
                person.MoveTo(Islands, pIsland, Random);
            }
        }
        void OrderAllEnemy(Classes.Job pJob, Classes.Island pIsland, Point pTargetPoint)
        {
            foreach (Classes.Person person in EnemyPop.Jobs(pJob))
            {
                person.MoveTo(Islands, pIsland, Random);
            }
        }
        void Building_Completion(object sender, EventArgs e)
        {
            if (SoundOn)
                BuildingDone.Play();
            if (((Classes.Building)sender).Type != Classes.BuildingType.Tree)
            {
                ((Classes.Building)sender).Completion -= Building_Completion;
                if (((Classes.Building)sender).Type != Classes.BuildingType.FallingTree)
                {
                    BuildingsInProgress.Remove((Classes.Building)sender);
                }
            }
            switch (((Classes.Building)sender).Type)
            {
                case Classes.BuildingType.Barracks:
                    God.FinishedBarracks();
                    break;
                case Classes.BuildingType.House:
                    God.FinishedHouse();
                    break;
                case Classes.BuildingType.Bridge:
                    God.FinishedBridge();
                    if (EnemyPop == null)
                        PlayerPop.OwnedIslands.Add(Islands[Islands.Count - 1]);
                    else if (Music.Playing)
                    {
                        Music.Stop();
                        CombatMusic.Play();
                    }
                    break;
                case Classes.BuildingType.Workshop:
                    God.FinishedWorkshop();
                    break;
                case Classes.BuildingType.Temple:
                    God.FinishedTemple();
                    break;
                case Classes.BuildingType.GreenHouse:
                    God.FinishedGreenHouse();
                    break;
                case Classes.BuildingType.Tree:
                    break;
                case Classes.BuildingType.FallingTree:
                    break;
            }
        }
        void Click(object sender, EventArgs e)
        {
            if (sender == btnMenu || sender == btnDo || sender == btnBuild || sender == btnTrain || sender == btnUntrain)
            {
                if (CurrentButton == (Classes.Button)sender)
                    CurrentButton = null;
                else
                    CurrentButton = (Classes.Button)sender;
            }
            else
            {
                #region Autres bouttons

                #region menu
                if (CurrentButton == btnMenu)
                {
                    if (sender == btnExit)
                        Order("exit");
                    else if (sender == btnNewTiny)
                        Order("new,tiny");
                    else if (sender == btnNewShort)
                        Order("new,short");
                    else if (sender == btnNewMedium)
                        Order("new,medium");
                    else if (sender == btnNewLong)
                        Order("new,long");
                    else if (sender == btnMusic)
                    {
                        if (CombatMusic.Playing || Music.Playing)
                        {
                            CombatMusic.Stop();
                            Music.Stop();
                        }
                        else
                        {
                            if (EnemyPop != null && PlayerPop.OwnedIslands[PlayerPop.OwnedIslands.Count - 1].Buildings.FirstOrDefault(t => t.Type == Classes.BuildingType.Bridge).ConstructionLevel <= 0)
                                CombatMusic.Play();
                            else
                                Music.Play();
                        }
                    }
                    else if (sender == btnSound)
                        SoundOn = !SoundOn;
                }
                #endregion
                #region do
                if (CurrentButton == btnDo)
                {
                    if (sender == btnForestation)
                        Order("do,forestation");
                    else if (sender == btnDeforestation)
                        Order("do,deforestation");
                    else if (sender == btnSacrifice)
                        Order("do,sacrifice");
                    else if (sender == btnAttack)
                        Order("do,attack");
                    else if (sender == btnConvert)
                        Order("do,convert");
                    else if (sender == btnRetreat)
                        Order("do,retreat");
                    else if (sender == btnPray)
                        Order("do,pray");
                    else if (sender == btnBabies)
                        Order("do,babies");
                    else if (sender == btnSummon)
                        Order("do,summon");
                }
                #endregion
                #region build
                if (CurrentButton == btnBuild)
                {
                    if (sender == btnHouse)
                        Order("build,house");
                    else if (sender == btnBarracks)
                        Order("build,barracks");
                    else if (sender == btnTemple)
                        Order("build,temple");
                    else if (sender == btnWorkshop)
                        Order("build,workshop");
                    else if (sender == btnGreenhouse)
                        Order("build,greenhouse");
                    else if (sender == btnBridge)
                        Order("build,bridge");
                }
                #endregion
                #region train
                if (CurrentButton == btnTrain)
                {
                    if (sender == btnTrainWarrior)
                        Order("train,warrior");
                    else if (sender == btnTrainBuilder)
                        Order("train,builder");
                    else if (sender == btnTrainPriest)
                        Order("train,priest");
                }
                #endregion
                #region untrain
                if (CurrentButton == btnUntrain)
                {
                    if (sender == btnUntrainWarrior)
                        Order("untrain,warrior");
                    else if (sender == btnUntrainBuilder)
                        Order("untrain,builder");
                    else if (sender == btnUntrainPriest)
                        Order("untrain,priest");
                }
                #endregion

                #endregion
            }
            if (sender == btnPause)
                Pause = !Pause;
        }
        void Order(string pOrder)
        {
            // This is here that parsing for commands is done
            // TODO :SHEIIIIT
            string[] text = pOrder.Split(',');
            int attempts, a;
            Point location;
            bool foundLocation;
            Classes.Building b;
            Classes.Island island;
            Classes.Person pers;
            #region Actual parsing and doing stuff

            if (text.Length > 0)
            {
                // 1 args lines
                #region new

                if (text[0] == "new")
                {
                    if (text.Length > 1)
                    {
                        #region tiny

                        if (text[1] == "tiny")
                        {
                            Goal = TinyGoal;
                            NewGame(true);
                            if (CombatMusic.Playing)
                            {
                                CombatMusic.Stop();
                                Music.Play();
                            }
                        }

                        #endregion
                        #region short

                        if (text[1] == "short")
                        {
                            Goal = ShortGoal;
                            NewGame(true);
                            if (CombatMusic.Playing)
                            {
                                CombatMusic.Stop();
                                Music.Play();
                            }
                        }

                        #endregion
                        #region medium

                        if (text[1] == "medium")
                        {
                            Goal = MediumGoal;
                            NewGame(true);
                            if (CombatMusic.Playing)
                            {
                                CombatMusic.Stop();
                                Music.Play();
                            }
                        }

                        #endregion
                        #region long

                        if (text[1] == "long")
                        {
                            Goal = LongGoal;
                            NewGame(true);
                            if (CombatMusic.Playing)
                            {
                                CombatMusic.Stop();
                                Music.Play();
                            }
                        }

                        #endregion
                    }
                    else if (text.Length == 1)
                    {
                        NewGame(true);
                        if (CombatMusic.Playing)
                        {
                            CombatMusic.Stop();
                            Music.Play();
                        }
                    }
                }

                #endregion
                #region exit

                if (text[0] == "exit" && text.Length == 1)
                    Exit();

                #endregion
                #region do

                if (text[0] == "do")
                {
                    if (text.Length > 1)
                    {
                        #region forestation

                        if (text[1] == "forestation")
                        {
                            if (SinceTree > 0)
                            {
                            }
                            else
                            {
                                a = 0;
                                attempts = 0;
                                foundLocation = false;
                                location = Point.Zero;
                                while (attempts < 20 && !foundLocation)
                                {
                                    // Try a random spot on a random island
                                    a = Random.Next(0, PlayerPop.OwnedIslands.Count);
                                    location = Classes.Person.GetRandomPoint(PlayerPop.OwnedIslands[a], Random, TreeRadius / 3);
                                    // Check its validity
                                    foreach (Classes.Building building in PlayerPop.OwnedIslands[a].Buildings)
                                        if (Classes.Person.Distance(new Vector2(location.X, location.Y), new Vector2(building.X, building.Y)) < building.Radius / 5 + TreeRadius / 5 &&
                                            building.Type != Classes.BuildingType.Bridge)
                                            location = Point.Zero;
                                    if (location == Point.Zero)
                                        attempts++;
                                    else
                                        foundLocation = true;
                                }
                                if (foundLocation)
                                {
                                    b = new Classes.Building(location.X, location.Y, TreeRadius, TreeGrowthTime, Classes.BuildingType.Tree, Tree);
                                    GodPoint.X = b.X;
                                    GodPoint.Y = b.Y;
                                    PlayerPop.OwnedIslands[a].Buildings.Add(b);
                                    b.Completion += new Classes.CompletionEventHandler(Building_Completion);
                                    if (SoundOn)
                                        BuildingStarted.Play();
                                    SinceTree = DoTree / (PlayerPop.Buildings(Classes.BuildingType.GreenHouse).Count + 1);
                                    God.FinishedTree();
                                }
                                else
                                {
                                }
                            }
                        }

                        #endregion
                        #region deforestation

                        if (text[1] == "deforestation")
                        {
                            a = PlayerPop.Buildings(Classes.BuildingType.Tree).Count;
                            if (a > 0)
                            {
                                b = PlayerPop.Buildings(Classes.BuildingType.Tree)[Random.Next(a)];
                                GodPoint.X = b.X;
                                GodPoint.Y = b.Y;
                                b.Type = Classes.BuildingType.FallingTree;
                                b.Texture = FallingTree;
                                b.ConstructionLevel = TreeCutTime;
                                if (SoundOn)
                                    BuildingStarted.Play();
                                God.FinishedCuttingTree();
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region sacrifice

                        if (text[1] == "sacrifice")
                        {
                            if (PlayerPop.People.Count > 1)
                            {
                                a = Random.Next(PlayerPop.People.Count);
                                GodPoint.X = (int)PlayerPop.People[a].X;
                                GodPoint.Y = (int)PlayerPop.People[a].Y;
                                Flash = 150;
                                SFX.Add(new Classes.SpecialEffect((int)PlayerPop.People[a].X, (int)PlayerPop.People[a].Y, 0, Lightning, true));
                                SFX.Add(new Classes.SpecialEffect((int)PlayerPop.People[a].X, (int)PlayerPop.People[a].Y, 0, SpecialEffects, false));
                                PlayerPop.People.RemoveAt(a);
                                God.Sacrificed();
                                if (SoundOn)
                                    Kill.Play();
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region attack

                        if (text[1] == "attack")
                        {
                            b = PlayerPop.OwnedIslands[PlayerPop.OwnedIslands.Count - 1].Buildings.FirstOrDefault(t => t.Type == Classes.BuildingType.Bridge);
                            if (b != null && b.ConstructionLevel <= 0)
                            {
                                GodPoint.X = b.X + 240;
                                GodPoint.Y = b.Y;
                                island = EnemyPop.OwnedIslands[Random.Next(EnemyPop.OwnedIslands.Count)];
                                OrderAll(Classes.Job.Warrior, island);
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region convert

                        if (text[1] == "convert")
                        {
                            b = PlayerPop.OwnedIslands[PlayerPop.OwnedIslands.Count - 1].Buildings.FirstOrDefault(t => t.Type == Classes.BuildingType.Bridge);
                            if (b != null && b.ConstructionLevel <= 0)
                            {
                                GodPoint.X = b.X + 240;
                                GodPoint.Y = b.Y;
                                island = EnemyPop.OwnedIslands[Random.Next(EnemyPop.OwnedIslands.Count)];
                                OrderAll(Classes.Job.Priest, island);
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region retreat

                        if (text[1] == "retreat")
                        {
                            b = PlayerPop.OwnedIslands[PlayerPop.OwnedIslands.Count - 1].Buildings.FirstOrDefault(t => t.Type == Classes.BuildingType.Bridge);
                            if (b != null && b.ConstructionLevel <= 0)
                            {
                                GodPoint.X = b.X - 240;
                                GodPoint.Y = b.Y;
                                island = PlayerPop.OwnedIslands[Random.Next(PlayerPop.OwnedIslands.Count)];
                                OrderAll(Classes.Job.Warrior, island);
                                OrderAll(Classes.Job.Villager, island);
                                OrderAll(Classes.Job.Builder, island);
                                OrderAll(Classes.Job.Priest, island);
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region pray

                        if (text[1] == "pray")
                        {
                            if (PlayerPop.People.Count > 0 && PlayerPop.People[0].Praying <= 0)
                            {
                                foreach (Classes.Person person in PlayerPop.People)
                                    person.Praying = Random.Next(75, 125);
                                God.Prayed();
                                if (SoundOn)
                                    Pray.Play();
                            }
                        }
                        #endregion
                        #region babies

                        if (text[1] == "babies")
                        {
                            int villagers = PlayerPop.Persons;
                            List<Classes.Person> babies = new List<Classes.Person>();
                            foreach (Classes.Person person in PlayerPop.People)
                                if (person.Job == Classes.Job.Villager && Random.Next(0, DoBabies) < person.SinceLastBaby)
                                {
                                    babies.Add(new Classes.Person((int)person.X, (int)person.Y, Villager, person.CurrentIsland, PlayerPop, false));
                                    GodPoint.X = (int)person.X;
                                    GodPoint.Y = (int)person.Y;
                                }
                            foreach (Classes.Person baby in babies)
                                if (PlayerPop.Persons < maxPeople)
                                    PlayerPop.People.Add(baby);
                            villagers = PlayerPop.Persons - villagers;
                            if (villagers > 0)
                            {
                            }
                            if (PlayerPop.Persons >= maxPeople)
                            {
                            }
                            if (babies.Count <= 0)
                            {
                                if (PlayerPop.People.FirstOrDefault(t => t.Job == Classes.Job.Villager) == null)
                                {
                                }
                                else if (PlayerPop.Persons < maxPeople)
                                {
                                }
                            }
                            else if (villagers > 0)
                            {
                                if (SoundOn)
                                    Babies.Play();
                                foreach (Classes.Person person in PlayerPop.People)
                                    person.SinceLastBaby = 0;
                            }
                            God.BabiesMade(villagers);
                        }
                        #endregion
                        #region summon

                        if (text[1] == "summon")
                        {
                            int villagers = PlayerPop.Summons;
                            List<Classes.Person> summoned = new List<Classes.Person>();
                            foreach (Classes.Person person in PlayerPop.People)
                            {
                                if (person.Job == Classes.Job.Priest && Random.Next(0, DoSummon) < person.SinceLastSummon)
                                {
                                    SFX.Add(new Classes.SpecialEffect((int)person.X, (int)person.Y, 2, SpecialEffects, false));
                                    summoned.Add(new Classes.Person((int)person.X, (int)person.Y, Villager, person.CurrentIsland, PlayerPop, true));
                                    GodPoint.X = (int)person.X;
                                    GodPoint.Y = (int)person.Y;
                                }
                            }
                            foreach (Classes.Person summon in summoned)
                                if (PlayerPop.Summons < maxSummon)
                                    PlayerPop.People.Add(summon);
                            villagers = PlayerPop.Summons - villagers;
                            if (villagers > 0)
                            {
                            }
                            if (PlayerPop.Summons >= maxSummon)
                            {
                            }
                            if (summoned.Count <= 0)
                            {
                                if (PlayerPop.People.FirstOrDefault(t => t.Job == Classes.Job.Priest) == null)
                                {
                                }
                                else if (PlayerPop.Summons < maxSummon)
                                {
                                }
                            }
                            else if (villagers > 0)
                            {
                                if (SoundOn)
                                    Summon.Play();
                                foreach (Classes.Person person in PlayerPop.People)
                                    person.SinceLastSummon = 0;
                            }
                            God.Summoned(villagers);
                        }
                        #endregion
                    }
                }

                #endregion
                #region build

                if (text[0] == "build")
                {
                    if (text.Length > 1)
                    {
                        #region house

                        if (text[1] == "house")
                        {
                            if (!BuildingInProgress)
                            {
                                a = 0;
                                attempts = 0;
                                foundLocation = false;
                                location = Point.Zero;
                                while (attempts < 20 * PlayerPop.OwnedIslands.Count && !foundLocation)
                                {
                                    // Try a random spot on a random island
                                    a = Random.Next(0, PlayerPop.OwnedIslands.Count);
                                    location = Classes.Person.GetRandomPoint(PlayerPop.OwnedIslands[a], Random, HouseRadius / 3);
                                    // Check its validity
                                    foreach (Classes.Building building in PlayerPop.OwnedIslands[a].Buildings)
                                        if (Classes.Person.Distance(new Vector2(location.X, location.Y), new Vector2(building.X, building.Y)) < building.Radius / 5 + HouseRadius / 5 &&
                                            building.Type != Classes.BuildingType.Bridge)
                                            location = Point.Zero;
                                    if (location == Point.Zero)
                                        attempts++;
                                    else
                                        foundLocation = true;
                                }
                                if (foundLocation)
                                {
                                    b = new Classes.Building(location.X, location.Y, HouseRadius, HouseConstructionTime, Classes.BuildingType.House, House);
                                    GodPoint.X = b.X;
                                    GodPoint.Y = b.Y;
                                    PlayerPop.OwnedIslands[a].Buildings.Add(b);
                                    BuildingsInProgress.Add(b);
                                    a = 0;
                                    while (a < 3)
                                    {
                                        pers = PlayerPop.Jobs(Classes.Job.Builder).FirstOrDefault(t => t.Building == null || t.Building != null && t.Building.ConstructionLevel <= 0);
                                        if (pers != null)
                                        {
                                            pers.Building = b;
                                            a++;
                                        }
                                        else
                                            a = 3;
                                    }
                                    b.Completion += new Classes.CompletionEventHandler(Building_Completion);
                                    if (SoundOn)
                                        BuildingStarted.Play();
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region barracks

                        if (text[1] == "barracks")
                        {
                            if (!BuildingInProgress)
                            {
                                a = 0;
                                attempts = 0;
                                foundLocation = false;
                                location = Point.Zero;
                                while (attempts < 20 * PlayerPop.OwnedIslands.Count && !foundLocation)
                                {
                                    // Try a random spot on a random island
                                    a = Random.Next(0, PlayerPop.OwnedIslands.Count);
                                    location = Classes.Person.GetRandomPoint(PlayerPop.OwnedIslands[a], Random, BarracksRadius / 3);
                                    // Check its validity
                                    foreach (Classes.Building building in PlayerPop.OwnedIslands[a].Buildings)
                                        if (Classes.Person.Distance(new Vector2(location.X, location.Y), new Vector2(building.X, building.Y)) < building.Radius / 5 + BarracksRadius / 5 &&
                                            building.Type != Classes.BuildingType.Bridge)
                                            location = Point.Zero;
                                    if (location == Point.Zero)
                                        attempts++;
                                    else
                                        foundLocation = true;
                                }
                                if (foundLocation)
                                {
                                    b = new Classes.Building(location.X, location.Y, BarracksRadius, BarracksConstructionTime, Classes.BuildingType.Barracks, Barracks);
                                    GodPoint.X = b.X;
                                    GodPoint.Y = b.Y;
                                    PlayerPop.OwnedIslands[a].Buildings.Add(b);
                                    BuildingsInProgress.Add(b);
                                    a = 0;
                                    while (a < 5)
                                    {
                                        pers = PlayerPop.Jobs(Classes.Job.Builder).FirstOrDefault(t => t.Building != null && t.Building.ConstructionLevel <= 0 || t.Building == null);
                                        if (pers != null)
                                        {
                                            pers.Building = b;
                                            a++;
                                        }
                                        else
                                            a = 5;
                                    }
                                    b.Completion += new Classes.CompletionEventHandler(Building_Completion);
                                    if (SoundOn)
                                        BuildingStarted.Play();
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region temple

                        if (text[1] == "temple")
                        {
                            if (!BuildingInProgress)
                            {
                                a = 0;
                                attempts = 0;
                                foundLocation = false;
                                location = Point.Zero;
                                while (attempts < 20 * PlayerPop.OwnedIslands.Count && !foundLocation)
                                {
                                    // Try a random spot on a random island
                                    a = Random.Next(0, PlayerPop.OwnedIslands.Count);
                                    location = Classes.Person.GetRandomPoint(PlayerPop.OwnedIslands[a], Random, TempleRadius / 3);
                                    // Check its validity
                                    foreach (Classes.Building building in PlayerPop.OwnedIslands[a].Buildings)
                                        if (Classes.Person.Distance(new Vector2(location.X, location.Y), new Vector2(building.X, building.Y)) < building.Radius / 5 + TempleRadius / 5 &&
                                            building.Type != Classes.BuildingType.Bridge)
                                            location = Point.Zero;
                                    if (location == Point.Zero)
                                        attempts++;
                                    else
                                        foundLocation = true;
                                }
                                if (foundLocation)
                                {
                                    b = new Classes.Building(location.X, location.Y, TempleRadius, TempleConstructionTime, Classes.BuildingType.Temple, Temple);
                                    GodPoint.X = b.X;
                                    GodPoint.Y = b.Y;
                                    PlayerPop.OwnedIslands[a].Buildings.Add(b);
                                    BuildingsInProgress.Add(b);
                                    a = 0;
                                    while (a < 5)
                                    {
                                        pers = PlayerPop.Jobs(Classes.Job.Builder).FirstOrDefault(t => t.Building != null && t.Building.ConstructionLevel <= 0 || t.Building == null);
                                        if (pers != null)
                                        {
                                            pers.Building = b;
                                            a++;
                                        }
                                        else
                                            a = 5;
                                    }
                                    b.Completion += new Classes.CompletionEventHandler(Building_Completion);
                                    if (SoundOn)
                                        BuildingStarted.Play();
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region workshop

                        if (text[1] == "workshop")
                        {
                            if (!BuildingInProgress)
                            {
                                a = 0;
                                attempts = 0;
                                foundLocation = false;
                                location = Point.Zero;
                                while (attempts < 20 * PlayerPop.OwnedIslands.Count && !foundLocation)
                                {
                                    // Try a random spot on a random island
                                    a = Random.Next(0, PlayerPop.OwnedIslands.Count);
                                    location = Classes.Person.GetRandomPoint(PlayerPop.OwnedIslands[a], Random, WorkshopRadius / 3);
                                    // Check its validity
                                    foreach (Classes.Building building in PlayerPop.OwnedIslands[a].Buildings)
                                        if (Classes.Person.Distance(new Vector2(location.X, location.Y), new Vector2(building.X, building.Y)) < building.Radius / 5 + WorkshopRadius / 5 &&
                                            building.Type != Classes.BuildingType.Bridge)
                                            location = Point.Zero;
                                    if (location == Point.Zero)
                                        attempts++;
                                    else
                                        foundLocation = true;
                                }
                                if (foundLocation)
                                {
                                    b = new Classes.Building(location.X, location.Y, WorkshopRadius, WorkshopConstructionTime, Classes.BuildingType.Workshop, Workshop);
                                    GodPoint.X = b.X;
                                    GodPoint.Y = b.Y;
                                    PlayerPop.OwnedIslands[a].Buildings.Add(b);
                                    a = 0;
                                    while (a < 5)
                                    {
                                        pers = PlayerPop.Jobs(Classes.Job.Builder).FirstOrDefault(t => t.Building != null && t.Building.ConstructionLevel <= 0 || t.Building == null);
                                        if (pers != null)
                                        {
                                            pers.Building = b;
                                            a++;
                                        }
                                        else
                                            a = 5;
                                    }
                                    BuildingsInProgress.Add(b);
                                    b.Completion += new Classes.CompletionEventHandler(Building_Completion);
                                    if (SoundOn)
                                        BuildingStarted.Play();
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region greenhouse

                        if (text[1] == "greenhouse")
                        {
                            if (!BuildingInProgress)
                            {
                                a = 0;
                                attempts = 0;
                                foundLocation = false;
                                location = Point.Zero;
                                while (attempts < 20 * PlayerPop.OwnedIslands.Count && !foundLocation)
                                {
                                    // Try a random spot on a random island
                                    a = Random.Next(0, PlayerPop.OwnedIslands.Count);
                                    location = Classes.Person.GetRandomPoint(PlayerPop.OwnedIslands[a], Random, GreenHouseRadius / 3);
                                    // Check its validity
                                    foreach (Classes.Building building in PlayerPop.OwnedIslands[a].Buildings)
                                        if (Classes.Person.Distance(new Vector2(location.X, location.Y), new Vector2(building.X, building.Y)) < building.Radius / 5 + GreenHouseRadius / 5 &&
                                            building.Type != Classes.BuildingType.Bridge)
                                            location = Point.Zero;
                                    if (location == Point.Zero)
                                        attempts++;
                                    else
                                        foundLocation = true;
                                }
                                if (foundLocation)
                                {
                                    b = new Classes.Building(location.X, location.Y, GreenHouseRadius, GreenHouseConstructionTime, Classes.BuildingType.GreenHouse, GreenHouse);
                                    GodPoint.X = b.X;
                                    GodPoint.Y = b.Y;
                                    PlayerPop.OwnedIslands[a].Buildings.Add(b);
                                    a = 0;
                                    while (a < 5)
                                    {
                                        pers = PlayerPop.Jobs(Classes.Job.Builder).FirstOrDefault(t => t.Building != null && t.Building.ConstructionLevel <= 0 || t.Building == null);
                                        if (pers != null)
                                        {
                                            pers.Building = b;
                                            a++;
                                        }
                                        else
                                            a = 5;
                                    }
                                    BuildingsInProgress.Add(b);
                                    b.Completion += new Classes.CompletionEventHandler(Building_Completion);
                                    if (SoundOn)
                                        BuildingStarted.Play();
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region bridge

                        if (text[1] == "bridge")
                        {
                            if (!BuildingInProgress)
                            {
                                if (PlayerPop.OwnedIslands.Contains(Islands[Islands.Count - 1]) && EnemyPop == null)
                                {
                                    b = new Classes.Building(Islands[Islands.Count - 1].ZoneX + 238, Islands[Islands.Count - 1].ZoneY, BridgeRadius, BridgeConstructionTime, Classes.BuildingType.Bridge, Bridge);
                                    GodPoint.X = b.X;
                                    GodPoint.Y = b.Y;
                                    Islands[Islands.Count - 1].Buildings.Add(b);
                                    if (Random.Next(0, 100) < Math.Min((PlayerPop.OwnedIslands.Count - 1) * 20, 90))
                                    {
                                        EnemyPop = new Classes.Populace(new Color(171, 23, 5));
                                        island = new Classes.Island(Islands, 480 * Islands.Count, Islands[Islands.Count - 1].Y, Math.Min(PlayerPop.OwnedIslands.Count, 20), Island, Clouds, Tree, Villager, Builder, Warrior, Priest, House, Workshop, Temple, Barracks, Bridge, EnemyPop, Random);
                                        Islands.Add(island);
                                        while (Random.Next(100) < Math.Min(10 + ((PlayerPop.OwnedIslands.Count - 1) * 10), 80))
                                        {
                                            island.Buildings.Add(new Classes.Building(island.ZoneX + 238, island.ZoneY, BridgeRadius, BridgeConstructionTime, Classes.BuildingType.Bridge, Bridge));
                                            island.Buildings.FirstOrDefault(t => t.Type == Classes.BuildingType.Bridge).ConstructionLevel = 0;
                                            island = new Classes.Island(Islands, 480 * Islands.Count, Islands[Islands.Count - 1].Y, PlayerPop.OwnedIslands.Count, Island, Clouds, Tree, Villager, Builder, Warrior, Priest, House, Workshop, Temple, Barracks, Bridge, EnemyPop, Random);
                                            Islands.Add(island);
                                        }

                                    }
                                    else
                                        Islands.Add(new Classes.Island(480 * Islands.Count, Islands[Islands.Count - 1].Y, Island, Clouds, Tree, Random));
                                    BuildingsInProgress.Add(b);
                                    a = 0;
                                    while (a < 5)
                                    {
                                        pers = PlayerPop.Jobs(Classes.Job.Builder).FirstOrDefault(t => t.Building != null && t.Building.ConstructionLevel <= 0 || t.Building == null);
                                        if (pers != null)
                                        {
                                            pers.Building = b;
                                            a++;
                                        }
                                        else
                                            a = 5;
                                    }
                                    b.Completion += new Classes.CompletionEventHandler(Building_Completion);
                                    island = PlayerPop.OwnedIslands[PlayerPop.OwnedIslands.Count - 1];
                                    if (SoundOn)
                                        BuildingStarted.Play();
                                }
                                else
                                {
                                    if (EnemyPop == null)
                                    {
                                        b = new Classes.Building(PlayerPop.OwnedIslands[PlayerPop.OwnedIslands.Count - 1].ZoneX + 238, 
                                                                 PlayerPop.OwnedIslands[PlayerPop.OwnedIslands.Count - 1].ZoneY, 
                                                                 BridgeRadius, BridgeConstructionTime, Classes.BuildingType.Bridge, Bridge);
                                        GodPoint.X = b.X;
                                        GodPoint.Y = b.Y;
                                        PlayerPop.OwnedIslands[PlayerPop.OwnedIslands.Count - 1].Buildings.Add(b);
                                        BuildingsInProgress.Add(b);
                                        a = 0;
                                        while (a < 5)
                                        {
                                            pers = PlayerPop.Jobs(Classes.Job.Builder).FirstOrDefault(t => t.Building != null && t.Building.ConstructionLevel <= 0 || t.Building == null);
                                            if (pers != null)
                                            {
                                                pers.Building = b;
                                                a++;
                                            }
                                            else
                                                a = 5;
                                        }
                                        b.Completion += new Classes.CompletionEventHandler(Building_Completion);
                                        if (SoundOn)
                                            BuildingStarted.Play();
                                    }
                                }
                            }
                            else
                            {
                            }
                        }

                        #endregion
                    }
                }

                #endregion
                #region train

                if (text[0] == "train")
                {
                    if (text.Length > 1)
                    {
                        #region warrior

                        if (text[1] == "warrior")
                        {
                            Classes.Person person = PlayerPop.People.FirstOrDefault(t => t.Job == Classes.Job.Villager);
                            if (person != null)
                            {
                                GodPoint.X = (int)person.X;
                                GodPoint.Y = (int)person.Y;
                                person.Job = Classes.Job.Warrior;
                                person.Texture = Warrior;
                                God.TrainedWarrior();
                                SFX.Add(new Classes.SpecialEffect((int)person.X, (int)person.Y, 1, SpecialEffects, false));
                                if (SoundOn)
                                    WarriorTrain.Play();
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region builder

                        if (text[1] == "builder")
                        {
                            Classes.Person person = PlayerPop.People.FirstOrDefault(t => t.Job == Classes.Job.Villager);
                            if (person != null)
                            {
                                GodPoint.X = (int)person.X;
                                GodPoint.Y = (int)person.Y;
                                person.Job = Classes.Job.Builder;
                                person.Texture = Builder;
                                God.TrainedBuilder();
                                SFX.Add(new Classes.SpecialEffect((int)person.X, (int)person.Y, 1, SpecialEffects, false));
                                if (SoundOn)
                                    BuilderTrain.Play();
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region priest

                        if (text[1] == "priest")
                        {
                            Classes.Person person = PlayerPop.People.FirstOrDefault(t => t.Job == Classes.Job.Villager);
                            if (person != null)
                            {
                                GodPoint.X = (int)person.X;
                                GodPoint.Y = (int)person.Y;
                                person.Job = Classes.Job.Priest;
                                person.Texture = Priest;
                                God.TrainedPriest();
                                SFX.Add(new Classes.SpecialEffect((int)person.X, (int)person.Y, 1, SpecialEffects, false));
                                if (SoundOn)
                                    PriestTrain.Play();
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region villager

                        if (text[1] == "villager")
                        {
                        }

                        #endregion
                    }
                }

                #endregion
                #region untrain

                if (text[0] == "untrain")
                {
                    if (text.Length > 1)
                    {
                        #region warrior

                        if (text[1] == "warrior")
                        {
                            Classes.Person person = PlayerPop.People.FirstOrDefault(t => t.Job == Classes.Job.Warrior);
                            if (person != null)
                            {
                                GodPoint.X = (int)person.X;
                                GodPoint.Y = (int)person.Y;
                                person.Job = Classes.Job.Villager;
                                person.Texture = Villager;
                                God.UntrainedWarrior();
                                SFX.Add(new Classes.SpecialEffect((int)person.X, (int)person.Y, 1, SpecialEffects, false));
                                if (SoundOn)
                                    VillagerTrain.Play();
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region builder

                        if (text[1] == "builder")
                        {
                            Classes.Person person = PlayerPop.People.FirstOrDefault(t => t.Job == Classes.Job.Builder);
                            if (person != null)
                            {
                                GodPoint.X = (int)person.X;
                                GodPoint.Y = (int)person.Y;
                                person.Job = Classes.Job.Villager;
                                person.Texture = Villager;
                                God.UntrainedBuilder();
                                SFX.Add(new Classes.SpecialEffect((int)person.X, (int)person.Y, 1, SpecialEffects, false));
                                if (SoundOn)
                                    VillagerTrain.Play();
                            }
                            else
                            {
                            }
                        }

                        #endregion
                        #region priest

                        if (text[1] == "priest")
                        {
                            Classes.Person person = PlayerPop.People.FirstOrDefault(t => t.Job == Classes.Job.Priest);
                            if (person != null)
                            {
                                GodPoint.X = (int)person.X;
                                GodPoint.Y = (int)person.Y;
                                person.Job = Classes.Job.Villager;
                                person.Texture = Villager;
                                God.UntrainedPriest();
                                SFX.Add(new Classes.SpecialEffect((int)person.X, (int)person.Y, 1, SpecialEffects, false));
                                if (SoundOn)
                                    VillagerTrain.Play();
                            }
                            else
                            {
                            }
                        }

                        #endregion
                    }
                }

                #endregion
            }

            #endregion
        }
    }
}