using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Devcade;

namespace DevcadeGame
{
    public enum GameState
    {
        Menu,
        Playing,
        Lost
    }

    public enum Direction
    {
        None,
        Left,
        Down,
        Right
    }

    public class Game1 : Game
    {

        public GameState gameState;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private TetrisGame game1, game2;
        private Menu menu;
        private InputManager inputManager;

        /// <summary>
        /// Game constructor
        /// </summary>
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
			gameState = GameState.Menu;
        }

        /// <summary>
        /// Does any setup prior to the first frame that doesn't need loaded content.
        /// </summary>
        protected override void Initialize()
        {
            Input.Initialize(); // Sets up the input library

            // Set window size if running debug (in release it will be fullscreen)
            #region
#if DEBUG
            _graphics.PreferredBackBufferWidth = 420;
            _graphics.PreferredBackBufferHeight = 980;
            _graphics.ApplyChanges();
#else
			_graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
			_graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
			_graphics.ApplyChanges();
#endif
            #endregion

            // TODO: Add your initialization logic here
            int seed = (new System.Random()).Next();
            game1 = new TetrisGame(seed, 1);
            game2 = new TetrisGame(seed, 2);
            inputManager = new InputManager(game1, game2, null);

            base.Initialize();
        }

        /// <summary>
        /// Does any setup prior to the first frame that needs loaded content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Renderer r = new Renderer(_spriteBatch, Content, _graphics);
            r.SetPlayers(1);
            r.LoadTextures();
            game1.Initialize(r);
            game2.Initialize(r);
        }

        /// <summary>
        /// Your main update loop. This runs once every frame, over and over.
        /// </summary>
        /// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
        protected override void Update(GameTime gameTime)
        {
            Input.Update(); // Updates the state of the input library

            // Exit when both menu buttons are pressed (or escape for keyboard debuging)
            // You can change this but it is suggested to keep the keybind of both menu
            // buttons at once for gracefull exit.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                (Input.GetButton(1, Input.ArcadeButtons.Menu) &&
                Input.GetButton(2, Input.ArcadeButtons.Menu)))
            {
                Exit();
            }

            int ticksSinceLastFrame = (int)(gameTime.ElapsedGameTime.TotalMilliseconds / (1000 / 60));

            game1.UpdatePre(ticksSinceLastFrame);
            game1.Update(ticksSinceLastFrame);
            inputManager.ProcessInput(ticksSinceLastFrame, gameState);
            game1.UpdatePost(ticksSinceLastFrame);

            base.Update(gameTime);
        }

        /// <summary>
        /// Your main draw loop. This runs once every frame, over and over.
        /// </summary>
        /// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            game1.Render();
            game2.Render();

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
