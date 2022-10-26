using Donkey_Kong.Enums;
using Donkey_Kong.GameObjects;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Donkey_Kong
{
    public class Game1 : Game
    {
        // Texture2D
        Texture2D EmptyTex;
        Texture2D BeamTex;
        Texture2D LadderTex;
        Texture2D LadderConnectorTex;
        Texture2D PlayerSpriteSheetTex;
        Texture2D DonkeyKongTex;
        Texture2D MenuScreenTex;
        Texture2D LoseScreenTex;
        Texture2D EnemyTex;
        Texture2D MiscellaneousTexturesSheat;

        // Float
        float TextLayer = 1.0f;
        float BackgroundLayer = 0.0f;
        float StructureLayer = 0.4f;
        float PlayerLayer = 0.8f;
        float EnemyLayer = 0.8f;
        float ItemLayer = 0.6f;
        float DeltaTime;
        float NextLevelDelayTime = 2;

        // Int
        int Height = 800;
        int Width = 800;
        int TileAmountX;
        int TileAmountY;
        int CurrentLevelIndex = 0;
        List<List<string>> CurrentLevel;

        // Tile
        Tile[,] TileMap;
        Tile DonkeyKong;
        Tile Pauline;

        // Rectangle
        Rectangle LifeTex = new(10, 230, 8, 8);
        Rectangle HeartTex = new(8, 202, 17, 13);
        Rectangle PaulineTex = new(18, 18, 16, 16);
        Rectangle TileSpecs;

        // Bool
        bool IsPauline = false;
        bool DoLevelProgression = false;

        // Keys
        List<Keys> ValidMenuInputs = new() { Keys.P, Keys.Enter, Keys.R};
        Keys? LastPressedKey;

        // Player vars
        int PlayerLives = 3;
        int playerScore = 0;
        Vector2 WalkSpeed = new(2, 0);
        Vector2 ClimbSpeed = new(0, 2);
        Vector2 Gravity = new(0, 4);
        List<Keys[]> ValidInputs = new() { new Keys[] { Keys.W, Keys.S, Keys.A, Keys.D, Keys.Space }, new Keys[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Space } };

        // Entities
        List<Enemy> SpawnedEnemies;
        Player PlayerChar;

        // Other
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        GameState CurrentState;
        SpriteFont GameFont;
        Timer NextLevelDelay;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            graphics.ApplyChanges();

            CurrentState = GameState.Menu;

            SpawnedEnemies = new();
            GameFont = Content.Load<SpriteFont>("GameFont");

            NextLevelDelay = new();

            InitializeTextures();

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateTimers(DeltaTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (CurrentState)
            {
                case GameState.Menu:

                    if (Keyboard.GetState().GetPressedKeys().Count() == 1 && Keyboard.GetState().GetPressedKeys()[0] != LastPressedKey)
                    {
                        switch (Keyboard.GetState().GetPressedKeys()[0])
                        {
                            case Keys.P:
                                IsPauline = !IsPauline;
                                break;
                            case Keys.Enter:
                                DoLevelProgression = true;
                                StartRestartLevel(false, DoLevelProgression);
                                break;
                            case Keys.R:
                                DoLevelProgression = false;
                                StartRestartLevel(false, DoLevelProgression);
                                break;
                        }
                    }
                    if (Keyboard.GetState().GetPressedKeys().Count() == 1)
                        LastPressedKey = Keyboard.GetState().GetPressedKeys()[0];
                    else
                        LastPressedKey = null;

                    break;
                case GameState.InGame:
                    UpdateEntities(DeltaTime);

                    if (PlayerChar.CurrentState == PlayerState.Dying)
                    {
                        DeactivateEnemies();
                        DespawnEnemies();
                    }

                    break;
                case GameState.LevelWin:
                    if (NextLevelDelay.IsDone())
                        StartRestartLevel(false, DoLevelProgression);
                    break;
                case GameState.GameLose:
                    if (ReplayCheck())
                        StartRestartLevel(true, DoLevelProgression);
                    break;
                case GameState.GameWin:
                    if (ReplayCheck())
                        RestartGame();
                    break;
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, Microsoft.Xna.Framework.Graphics.SamplerState.PointWrap);

            switch (CurrentState)
            {
                case GameState.Menu:
                    DrawMainMenu();
                    break;
                case GameState.InGame:
                    DrawLevel();
                    DrawEntities();
                    if (PlayerChar.CurrentState != PlayerState.Dying)
                        DrawHud();
                    break;
                case GameState.LevelWin:
                    DrawLevel();
                    DrawEntities();
                    DrawWinScreen();
                    break;
                case GameState.GameWin:
                    DrawEndScreen();
                    break;
                case GameState.GameLose:
                    DrawLoseScreen();
                    break;
            }

            spriteBatch.End();

            // TODO: Add your drawing code here
            base.Draw(gameTime);
        }

        /// <summary>
        /// Loads all the textures and initializes their respective variables
        /// </summary>
        public void InitializeTextures()
        {
            EmptyTex = Content.Load<Texture2D>("empty");
            BeamTex = Content.Load<Texture2D>("bridge");
            LadderTex = Content.Load<Texture2D>("ladder");
            LadderConnectorTex = Content.Load<Texture2D>("bridgeLadder");
            PlayerSpriteSheetTex = Content.Load<Texture2D>("mario-pauline");
            DonkeyKongTex = Content.Load<Texture2D>("DK_mod_mario");
            MenuScreenTex = Content.Load<Texture2D>("start");
            LoseScreenTex = Content.Load<Texture2D>("lose");
            EnemyTex = Content.Load<Texture2D>("stuff_mod");
            MenuScreenTex = Content.Load<Texture2D>("start");
            MiscellaneousTexturesSheat = Content.Load<Texture2D>("stuff_mod");
        }

        public void ReadLevel(bool continueFromLastLevel)
        {
            int levelWidth = 0;
            int currentRow = 0;

            List<List<string>> levelData = new();

            bool endGame;

            StreamReader reader = GetLevelFromFile(out endGame,continueFromLastLevel);
            string currentLine;

            if (endGame)
            {
                CurrentState = GameState.GameWin;
                return;
            }

            do
            {
                currentLine = reader.ReadLine();
                levelData.Add(new List<string>());
                if (currentLine.Length > levelWidth)
                    levelWidth = currentLine.Length;

                foreach (char cha in currentLine)
                {
                    levelData[currentRow].Add($"{cha}");
                }

                currentRow++;

            } while (!reader.EndOfStream);

            reader.Close();

            CurrentLevel = levelData;

            TileSpecs = new();

            TileSpecs.Width = Width / levelWidth;
            TileSpecs.Height = Height / levelData.Count;

            TileMap = new Tile[levelData.Count, levelWidth];

            TileAmountX = levelWidth;
            TileAmountY = levelData.Count;
        }

        /// <summary>
        /// Loads current level
        /// </summary>
        public void LoadLevel()
        {
            int?[] donkeykongPos = new int?[2];

            int donkeyKongHeight = 0;
            int donkeyKongWidth = 0;

            for (int y = 0; y < TileAmountY; y++)
            {
                int donkeyKongTempWidth = 0;
                for (int x = 0; x < TileAmountX; x++)
                {
                    Rectangle destinationRectangle = new(TileSpecs.Width * x, TileSpecs.Height * y, TileSpecs.Width, TileSpecs.Height);

                    if (TileAmountX >= CurrentLevel[y].Count())
                        CurrentLevel[y].Add("0");

                    TileMap[y, x] = TileMaker(int.Parse(CurrentLevel[y][x]), destinationRectangle, y, x, donkeyKongTempWidth, out donkeyKongTempWidth, donkeyKongHeight, out donkeyKongHeight, donkeykongPos, out donkeykongPos);
                }
                if (donkeyKongTempWidth > donkeyKongWidth)
                    donkeyKongWidth = donkeyKongTempWidth;
            }

            if (donkeykongPos[0] != null && donkeykongPos[1] != null)
            {
                Rectangle donkeyKongPosRec = TileMap[(int)donkeykongPos[0], (int)donkeykongPos[1]].DestinationRec;

                Rectangle donkeyKongRec = new Rectangle(donkeyKongPosRec.X, donkeyKongPosRec.Y, donkeyKongPosRec.Width * donkeyKongWidth, donkeyKongPosRec.Height * donkeyKongHeight);

                Tile donkeyKongTile = new(TileType.Empty, new Texture2D[] { EmptyTex, DonkeyKongTex }, donkeyKongPosRec, new float[] { BackgroundLayer, EnemyLayer }, null, donkeyKongRec);

                DonkeyKong = donkeyKongTile;

                TileMap[(int)donkeykongPos[0], (int)donkeykongPos[1]] = donkeyKongTile;
            }
        }

        /// <summary>
        /// Gets all levels from files
        /// </summary>
        /// <returns></returns>
        public StreamReader GetLevelFromFile(out bool gameEnd, bool getNextLevel = false)
        {
            List<StreamReader> levels = new();

            levels.Add(new(@"Levels/Level1.txt"));
            levels.Add(new(@"Levels/Level2.txt"));

            gameEnd = false;

            if (!getNextLevel)
                return levels[new Random().Next(levels.Count)];
            else if (CurrentLevelIndex < levels.Count && DoLevelProgression)
            {
                CurrentLevelIndex++;
                return levels[CurrentLevelIndex - 1];
            }
            else
            {
                gameEnd = true;
                return null;
            }
        }

        /// <summary>
        /// Draws all tiles of the level
        /// </summary>
        public void DrawLevel()
        {
            for (int y = 0; y < TileAmountY; y++)
            {
                for (int x = 0; x < TileAmountX; x++)
                {
                    TileMap[y, x].Draw(spriteBatch);
                }
            }
        }

        /// <summary>
        /// Draws player and enemies
        /// </summary>
        public void DrawEntities()
        {
            if (SpawnedEnemies.Count > 0)
                foreach (Enemy enemy in SpawnedEnemies)
                    enemy.Draw(spriteBatch);

            PlayerChar.Draw(spriteBatch);
        }

        /// <summary>
        /// Draws in game hud
        /// </summary>
        public void DrawHud()
        {
            string livesStr = "Lives: ";
            //string scoreStr = "Score:";
            //string scoreAmountStr = $"{playerScore}";

            Vector2 livesSize = MeasureString(livesStr) * 1.5f;
            //Vector2 scoreSize = MeasureString(scoreStr) * 1.2f;
            //Vector2 scoreAmountSize = MeasureString(scoreAmountStr) * 1.2f;

            Vector2 livesPos = Vector2.Zero;
            //Vector2 scorePos = new(Width - scoreSize.X, 0);
            //Vector2 scoreAmountPos = new(Width - scoreAmountSize.X, scoreSize.Y);

            //if (scoreSize.X >= scoreAmountSize.X)
            //    scoreAmountPos.X = Width - scoreSize.X / 2;

            //else
            //    scorePos.X = Width - scoreAmountSize.X / 2;

            DrawStringOnScreen(livesStr, livesPos, 1.5f);

            for (int x = 0; x < PlayerLives; x++)
            {
                Rectangle destinationRec = new((int)(livesPos.X + livesSize.X + (livesSize.Y * x)), (int)livesPos.Y, (int)livesSize.Y, (int)livesSize.Y);
                spriteBatch.Draw(MiscellaneousTexturesSheat, destinationRec, LifeTex, Color.White, 0f, new Vector2(), SpriteEffects.None, TextLayer);
            }


            //DrawStringOnScreen(scoreStr, scorePos, 1.2f);
            //DrawStringOnScreen(scoreAmountStr, scoreAmountPos, 1.2f);
        }

        public void DrawMainMenu()
        {
            int spriteWidth = (int)(Width * .075f);
            int spriteHeight = (int)(Height * .075f);

            Rectangle destRec = new(Width-spriteWidth,Height-spriteHeight, spriteWidth, spriteHeight);

            spriteBatch.Draw(MenuScreenTex, new Rectangle(0, 0, Width, Height), null, Color.White);

            if (IsPauline) 
                spriteBatch.Draw(PlayerSpriteSheetTex, destRec, PaulineTex, Color.White, 0f, new Vector2(), SpriteEffects.FlipHorizontally, TextLayer);
        }

        public void DrawWinScreen()
        {
            Rectangle heartlocation = new Rectangle(Pauline.DestinationRec.X, Pauline.DestinationRec.Y - TileSpecs.Height, TileSpecs.Width, TileSpecs.Height);
            spriteBatch.Draw(MiscellaneousTexturesSheat, heartlocation,HeartTex, Color.White, 0f, new Vector2(), SpriteEffects.None,ItemLayer);
        }

        public void DrawLoseScreen()
        {
            spriteBatch.Draw(LoseScreenTex, new Rectangle(0, 0, Width, Height), null, Color.White);
        }

        public void DrawEndScreen()
        {
            string gameWonStr = "You Won!";

            Vector2 gameWonSize = MeasureString(gameWonStr);

            Vector2 GameWonPos = new(Width / 2 - gameWonSize.X / 2, Height / 2 - gameWonSize.Y / 2);

            DrawStringOnScreen(gameWonStr, GameWonPos, 3f);
        }

        /// <summary>
        /// Updates player and enemies
        /// </summary>
        public void UpdateEntities(float deltaTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            Keys[] input = keyboard.GetPressedKeys();
            int? inputIndex = null;

            if (input.Length == 1)
                foreach (Keys[] inputScheme in ValidInputs)
                {
                    if (inputScheme.Contains(input[0]))
                    {
                        inputIndex = Array.IndexOf(inputScheme, input[0]);
                        break;
                    }
                }

            PlayerChar.Update(deltaTime, TileMap, out PlayerLives, EndGame, DespawnEnemies, inputIndex);
            DonkeyKong.Update(deltaTime);

            if (SpawnedEnemies.Count > 0)
                foreach (Enemy enemy in SpawnedEnemies)
                    enemy.Update(deltaTime, TileMap, PlayerChar);
        }

        /// <summary>
        /// Makes Tiles For the level
        /// </summary>
        /// <param name="tileTypeInt">Int corresponding to the tiles tiletype enum</param>
        /// <param name="destinationRec">Destination recatngle of tile</param>
        /// <param name="y">y index in TileMap array</param>
        /// <param name="x">x index in TileMap array</param>
        /// <param name="donkeyKongTempWidth">donkey kong temporary width</param>
        /// <param name="donkeyKongTempWidthOut">donkeyKongTempWidth output</param>
        /// <param name="donkeyKongHeight">height of donkey kong</param>
        /// <param name="donkeyKongHeightOut">donkeyKongHeight output</param>
        /// <param name="donkeykongPos"></param>
        /// <param name="donkeyKongPosOut">donkeykongPos output</param>
        /// <returns></returns>
        public Tile TileMaker(int tileTypeInt, Rectangle destinationRec, int y, int x, int donkeyKongTempWidth, out int donkeyKongTempWidthOut, int donkeyKongHeight, out int donkeyKongHeightOut, int?[] donkeykongPos, out int?[] donkeyKongPosOut)
        {
            Tile newTile;
            List<Texture2D> textures = new() { EmptyTex };
            List<float> drawLayers = new() { BackgroundLayer };

            switch (tileTypeInt)
            {
                case (int)TileType.Empty:
                    newTile = new(TileType.Empty, textures.ToArray(), destinationRec, drawLayers.ToArray());
                    break;
                case (int)TileType.Beam:
                    textures.Add(BeamTex);
                    drawLayers.Add(StructureLayer);
                    newTile = new(TileType.Beam, textures.ToArray(), destinationRec, drawLayers.ToArray());
                    break;
                case (int)TileType.Ladder:
                    textures.Add(LadderTex);
                    drawLayers.Add(StructureLayer);
                    newTile = new(TileType.Ladder, textures.ToArray(), destinationRec, drawLayers.ToArray());
                    break;
                case (int)TileType.LadderConnector:
                    textures.Add(LadderConnectorTex);
                    drawLayers.Add(StructureLayer);
                    newTile = new(TileType.LadderConnector, textures.ToArray(), destinationRec, drawLayers.ToArray());
                    break;
                case (int)TileType.DonkeyKong:
                    if (donkeykongPos[0] == null && donkeykongPos[1] == null)
                    {
                        donkeykongPos[0] = y;
                        donkeykongPos[1] = x;
                        donkeyKongTempWidth++;
                        donkeyKongHeight++;
                    }
                    else if (donkeykongPos[1] + donkeyKongTempWidth == x)
                        donkeyKongTempWidth++;

                    if (donkeykongPos[0] + donkeyKongHeight == y)
                        donkeyKongHeight++;

                    newTile = new(TileType.Empty, textures.ToArray(), destinationRec, drawLayers.ToArray());
                    break;
                case (int)TileType.Pauline:
                    Rectangle sourceRec;
                    textures.Add(PlayerSpriteSheetTex);
                    if (IsPauline)
                        sourceRec = new(18, 1, 16, 16);
                    else
                        sourceRec = new(18, 18, 16, 16);

                    drawLayers.Add(EnemyLayer);
                    newTile = new(TileType.Empty, textures.ToArray(), destinationRec, drawLayers.ToArray(), sourceRec);
                    Pauline = newTile;
                    break;
                case (int)TileType.PlayerSpawn:
                    newTile = new(TileType.Empty, textures.ToArray(), destinationRec, drawLayers.ToArray());
                    Vector2[] vectors = new Vector2[] { Gravity, WalkSpeed, ClimbSpeed };
                    PlayerChar = new(PlayerSpriteSheetTex, destinationRec, PlayerLayer, vectors, new int[2] { y, x }, PlayerLives, IsPauline);
                    break;
                case (int)TileType.EnemySpawn:
                    newTile = new(TileType.Empty, textures.ToArray(), destinationRec, drawLayers.ToArray());
                    Enemy newEnemy = new(EnemyTex, destinationRec, EnemyLayer, new int[2] { y, x });
                    SpawnedEnemies.Add(newEnemy);
                    break;
                case (int)TileType.GoalTile:
                    newTile = new(TileType.GoalTile, textures.ToArray(), destinationRec, drawLayers.ToArray());
                    break;
                case (int)TileType.OneUp:
                    textures.Add(MiscellaneousTexturesSheat);
                    drawLayers.Add(ItemLayer);
                    newTile = new(TileType.OneUp, textures.ToArray(), destinationRec, drawLayers.ToArray(), HeartTex);
                    break;
                default:
                    newTile = new(TileType.Empty, textures.ToArray(), destinationRec, drawLayers.ToArray());
                    break;
            }

            donkeyKongHeightOut = donkeyKongHeight;
            donkeyKongTempWidthOut = donkeyKongTempWidth;
            donkeyKongPosOut = donkeykongPos;

            return newTile;
        }

        /// <summary>
        /// Takes a string and returns it's height and width
        /// </summary>
        /// <param name="stringToMeasure">The string that is to be measured</param>
        /// <returns>Vector2 containing the length and width of the string</returns>
        public Vector2 MeasureString(string stringToMeasure)
        {
            return GameFont.MeasureString(stringToMeasure);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringToDraw">String that is to be drawn</param>
        /// <param name="drawPos">Position to draw</param>
        /// <param name="scale">Scale of text</param>
        public void DrawStringOnScreen(string stringToDraw, Vector2 drawPos, float scale)
        {
            spriteBatch.DrawString(GameFont, stringToDraw, drawPos, Color.White, 0f, new Vector2(), scale, new SpriteEffects(), TextLayer);
        }

        /// <summary>
        /// Updates all timers
        /// </summary>
        public void UpdateTimers(float deltaTime)
        {
            NextLevelDelay.Update(deltaTime);
        }

        /// <summary>
        /// Ends current game
        /// </summary>
        /// <param name="isGameWon">true if game is won, else false</param>
        public void EndGame(bool isLevelWon)
        {
            DeactivateEnemies();

            if (isLevelWon && DoLevelProgression)
            {
                CurrentState = GameState.LevelWin;
                NextLevelDelay.StartTimer(NextLevelDelayTime);
            }
            else if (!DoLevelProgression)
                CurrentState = GameState.GameWin;
            else
                CurrentState = GameState.GameLose;
        }

        /// <summary>
        /// Deactivates all spawned enemies
        /// </summary>
        public void DeactivateEnemies()
        {
            foreach (Enemy enemy in SpawnedEnemies)
                enemy.DeactivateEnemy();
        }

        public void DespawnEnemies()
        {
            SpawnedEnemies = new List<Enemy>();
        }

        public bool ReplayCheck()
        {
            return Keyboard.GetState().IsKeyDown(Keys.Enter);
        }

        public void StartRestartLevel(bool restart, bool continueFromLastLevel = false)
        {
            CurrentState = GameState.InGame;
            DespawnEnemies();
            if (restart)
                PlayerLives = 3;
            else
                ReadLevel(continueFromLastLevel);
            LoadLevel();
        }

        public void RestartGame()
        {
            CurrentLevelIndex = 0;
            StartRestartLevel(false, DoLevelProgression);
        }
    }
}