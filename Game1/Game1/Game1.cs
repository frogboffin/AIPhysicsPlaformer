using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace GameBehaviour
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        World world;
        Player player;
        AI bot;
        AIManager aiManager;

        private List<Texture2D> textures = new List<Texture2D>();
        private Texture2D bg;
        private Texture2D defaultSquare;
        public Texture2D debug;
        private float timer;
        Point screenCenter;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 800;   // set this value to the desired height of your window
            graphics.ApplyChanges();
            graphics.CreateDevice();

            Content.RootDirectory = "Content";
            
            this.IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(30); // 20 milliseconds, or 50 FPS.

            timer = 3;
            base.Initialize();

            world = new World(graphics.GraphicsDevice, textures); //Create a new world and pass the graphics device and textures

            //Using the World class create all the objects required for the level

            #region Player/Bot
            bot = (world.CreateAI(screenCenter.X, screenCenter.Y, defaultSquare));
            player = world.CreatePlayer(screenCenter.X, screenCenter.Y, defaultSquare);
            #endregion

            aiManager = new AIManager(bot, player, world);
            world.ai = aiManager;

            #region Obstacles
            world.AddBox("1", 220, screenCenter.Y / 2f, 50, 50, Color.White, 0.3f, 20f, false, true, false, true,0.3f);
            world.AddBox("2", 1030, screenCenter.Y / 2f, 50, 50, Color.White, 0.3f, 20f, false, true, false, true, 0.3f);
            world.AddBox("3", 220, screenCenter.Y / 3f, 50, 50, Color.White, 0.3f, 20f, false, true, false, true, 0.3f);
            world.AddBox("4", 1030, screenCenter.Y / 3f, 50, 50, Color.White, 0.3f, 20f, false, true, false, true, 0.3f);
            world.AddSpringJointGrid(new Vector2(600, 700), 10, 9, 1, true, null, false, true, 300, -30, 0.99f);
            world.AddSpringJointGrid(new Vector2(900, 540), 10, 3, 3, false, null, false, true, 8, -10, 0.59f);
            world.AddSpringJointGrid(new Vector2(350, 540), 10, 3, 3, false, null, false, true, 8, -10, 0.59f);

            #endregion

            #region Boundaries  
            world.AddBox("Floor", 0, (screenCenter.Y * 2) - 10, screenCenter.X * 2, 300, Color.Black, 0.2f, 100f, true, false, false, true);//bottom
            world.AddBox("left",-30, 0, 30, screenCenter.Y*2, Color.Black, 0.7f, 100f, true, false, true, false);//left
            world.AddBox("right",(screenCenter.X*2), 0, 30, screenCenter.Y*2, Color.Black, 0.3f, 100f, true, false, true, false);//right
            #endregion

            #region Platforms
            world.AddBox("BottomLeft", 0, (screenCenter.Y * 1.7f), 500, 30, Color.Green, 0.2f, 1000f, true, true, false);
            world.AddBox("TopLeft", 0, (screenCenter.Y * 1.3f), 300, 30, Color.Green, 0.2f, 1000f, true, true, false);

            world.AddBox("TopLeft", (screenCenter.X) - 150, (screenCenter.Y * 01f), 300, 30, Color.Green, 0.2f, 1000f, true, true, false);

            world.AddBox("BottomRight", (screenCenter.X * 2) - 500, (screenCenter.Y * 1.7f), 500, 30, Color.Green, 0.2f, 1000f, true, true, false);
            world.AddBox("TopRight", (screenCenter.X * 2) - 300, (screenCenter.Y * 1.3f), 300, 30, Color.Green, 0.2f, 1000f, true, true, false);
            #endregion

            #region Bucket
            world.AddBox("BucketLeft", screenCenter.X - 40, (screenCenter.Y * 0.3f), 10, 100, Color.Green, 0.2f, 1000f, true, true, false, false);
            world.AddBox("BucketRight", screenCenter.X + 40, (screenCenter.Y * 0.3f), 10, 100, Color.Green, 0.2f, 1000f, true, true, false, false);
            world.AddBox("BucketBottom", screenCenter.X - 40, (screenCenter.Y * 0.3f) + 100, 90, 10, Color.Green, 0.2f, 1000f, true, true, false, false);
            world.AddBox("Top", screenCenter.X - 40, (screenCenter.Y * 0.3f), 90, 10, Color.Green, 0.2f, 1000f, true, true, false, false);
            #endregion

            world.Initialise();
            aiManager.PopulateNodes(world.mStaticPropList);

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
        }

        protected override void LoadContent()
        {
            screenCenter = new Point(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height / 2); 
            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //Loading of all textures required
            bg = this.Content.Load<Texture2D>("BG");
            defaultSquare = this.Content.Load<Texture2D>("Square");
            textures.Add(debug = new Texture2D(GraphicsDevice, 1, 1));
            textures.Add(defaultSquare);
            textures.Add(this.Content.Load<Texture2D>("Ball2"));
            textures.Add(this.Content.Load<Texture2D>("Rain"));
            textures.Add(this.Content.Load<Texture2D>("BigSquare"));

            debug.SetData<Color>(new Color[] { Color.Blue *0.8f});// fill the texture with white
            DebugDraw.Instance.SetTexture(debug);// Set the debug instance texture to the single pixel so that we can draw debuglines
            
        }

        protected override void UnloadContent()
        {
            // Nothing needs to go here
        }

        protected override void Update(GameTime gameTime)
        {
            world.Update(gameTime);//Update the phsyics and timed events of the world

            aiManager.Update();//Update the pathing for the AI bot
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.D1)) DebugDraw.Instance.debugMode = true; //Toggle Debug mode drawing
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) DebugDraw.Instance.debugMode = false; //Toggle Debug mode drawing
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) graphics.ToggleFullScreen(); //Toggle Fullscreen
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) world.ToggleRain(); //Toggle the rain amount for performance 
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) world.RandomButton(); //Find new buttons for debugging
            if (Keyboard.GetState().IsKeyDown(Keys.D6)) world.Flip(); //Flips the gravity for no reason


            //player Controller 
            if (player != null)
            {
                player.Move(Input.Instance.GetAxis(), 20f);
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    //Console.WriteLine("Jumping");
                    player.Jump();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && timer >= player.DashCooldown)
                {
                    timer = 0;
                    player.Dash(Input.Instance.GetAxis());
                    //Console.WriteLine("Dashing");
                }
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Clear the screen
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            spriteBatch.Draw(bg, new Rectangle(0, 0, GraphicsDevice.Viewport.Width , GraphicsDevice.Viewport.Height), Color.White);

            //Draw the nodes and connections (only if debugmode is true)
            aiManager.DrawNodes(spriteBatch);

            spriteBatch.End();

            //Draw the world and all of its objects
            world.Draw(gameTime, spriteBatch);

            base.Draw(gameTime);
        }
    }
}
