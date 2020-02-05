using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using System.Diagnostics;

namespace Arcademenu
{
    public class Menu : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private InputManager inputManager;
        public static Menu instance;
        public static RunListener listener;

        public static readonly string basePath = @"C:\Users\Anwender\OneDrive\Dokumente\Arcaduino";
        public static readonly string gamesPath = basePath + @"\Games\";
        private List<string> games = new List<string>();
        private List<string> gameNames = new List<string>();
        private List<Texture2D> gameIcons = new List<Texture2D>();

        public Menu()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            instance = this;

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;

            graphics.SynchronizeWithVerticalRetrace = true;
            this.IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1 / 30f);
        }
        protected override void Initialize()
        {
            inputManager = new InputManager().Initialize();

            games.AddRange(Directory.GetFiles(gamesPath).Where(name => name.EndsWith(".lnk") || name.EndsWith(".url")));
            if (games.Count < displayedGames)
                games.AddRange(games.GetRange(0, displayedGames - games.Count));
            games.ForEach(game => gameNames.Add(Path.GetFileNameWithoutExtension(game)));
            foreach (string game in games)
            {
                try
                {
                    FileStream fileStream = new FileStream(Path.ChangeExtension(game, ".png"), FileMode.Open);
                    gameIcons.Add(Texture2D.FromStream(GraphicsDevice, fileStream));
                    fileStream.Dispose();
                }
                catch
                {
                    MemoryStream stream = new MemoryStream();
                    System.Drawing.Icon.ExtractAssociatedIcon(game).ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                    gameIcons.Add(Texture2D.FromStream(GraphicsDevice, stream));
                }
            }

            spriteBatch = new SpriteBatch(GraphicsDevice);
            base.Initialize();
        }
        protected override void LoadContent()
        {
            base.LoadContent();
            font = Content.Load<SpriteFont>("default");
        }
        protected override void Update(GameTime gameTime)
        {
            inputManager.Update(gameTime.DeltaTime());
            base.Update(gameTime);
        }

        private int displayedGames = 6;
        private int activeIndex = 2;
        private int scrollDelta = 0;
        private float activeUpscale = 1f;
        private int iconSize = 90;
        private int padding = 20;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            for (int i = 0; i < displayedGames; i++)
            {
                int offset = padding + i * (iconSize + padding) + (int)(i > activeIndex ? iconSize * activeUpscale : 0);
                int thisIconSize = (int)(i == activeIndex ? iconSize * (activeUpscale + 1) : iconSize);
                spriteBatch.Draw(gameIcons[(scrollDelta + i) % games.Count], new Rectangle(200, offset, thisIconSize, thisIconSize), Color.White);
            }
            spriteBatch.DrawString(font, gameNames[(scrollDelta + activeIndex) % games.Count], new Vector2(220 + iconSize * (activeUpscale + 1), padding + activeIndex * (iconSize + padding)), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Scroll(int direction)
        {
            if (direction == 0)
                return;
            scrollDelta += direction;
            if (scrollDelta < 0)
                scrollDelta += games.Count;
            if (scrollDelta >= games.Count)
                scrollDelta -= games.Count;
        }

        public void RunExe()
        {
            string path = games[(scrollDelta + activeIndex) % games.Count];
            Process.Start(path);
            listener.AppRunning(gameNames[(scrollDelta + activeIndex) % games.Count]);
        }
    }
}
