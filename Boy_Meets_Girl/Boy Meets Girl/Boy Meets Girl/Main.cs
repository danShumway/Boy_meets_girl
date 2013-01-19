using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Boy_Meets_Girl
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {

        #region variables

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Global variables.
        public static Random random;
        public const int width = 900;
        public const int height = 700;
        public static Color[] colors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.White };

        //Variables for game/display
        World world;
        BaseObject currentSelection;
        Vector2 spriteDimensions; //How large of a box to draw around each character.
        int indexSelected; //Which flower you've selected from your inventory.

        //Pure graphics
        SpriteFont titleScreen;
        SpriteFont flowerTexture;
        SpriteFont statsFont;
        Texture2D colorTexture;
        //Graphic dimensions - for spritefonts and positioning and the like.

        /// <summary>
        /// Width and height of the title
        /// </summary>
        Vector2 titleDimensions;

        /// <summary>
        /// Width and height of an on-screen character (for flowers and people).
        /// Static so it's accessible from world.
        /// </summary>
        public static Vector2 characterDimension;

        /// <summary>
        /// Width and height of a single character in the stats-font.
        /// </summary>
        Vector2 statsDimensions;

        #endregion

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Set screen resolution and cursor.
            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferWidth = width;
            this.IsMouseVisible = true;

            //Set up global random.
            random = new Random();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //INITIALIZE YOUR BASE NOW!
            //This makes sure that the textures and fonts will be loaded in before they're measured.
            base.Initialize();

            //Set up your spritefont and dimensions and stuff.
            titleDimensions = titleScreen.MeasureString("Boy Meets Girl");
            characterDimension = flowerTexture.MeasureString("X");
            statsDimensions = statsFont.MeasureString("X"); 

            //Set up the game world.
            //You'll notice that the right and bottom margins are adjust to make sure nothing spawns half way on the screen.
            world = new World(20, 
                (int)titleDimensions.Y, width - 40 - (int)characterDimension.X, 
                height - (int)titleDimensions.Y - 100 - (int)characterDimension.Y, 0, 3);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Fonts and textures.
            titleScreen = Content.Load<SpriteFont>("TitleScreen");
            flowerTexture = Content.Load<SpriteFont>("flowerSprite");
            colorTexture = Content.Load<Texture2D>("ColorTexture");
            statsFont = Content.Load <SpriteFont>("statsFont");

            //Measure the font to make sure everything is cool on that front.
            spriteDimensions = flowerTexture.MeasureString("X");
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

            #region interface and buttons

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Mouse tracking
            currentSelection = null;
            MouseState currentState = Mouse.GetState();
            //Loop through all the game objects.  See if you're overtop of any of them.
            for (int i = 0; i < world.objects.Count; ++i)
            {
                //Check position based on fontsize.
                if (currentState.X > world.objects[i].position.X && currentState.X < world.objects[i].position.X + spriteDimensions.X
                    && currentState.Y > world.objects[i].position.Y && currentState.Y < world.objects[i].position.Y + spriteDimensions.Y)
                {
                    currentSelection = world.objects[i];
                    break; //Forget you!  I'll use break statements as much as I want while hacking!
                }
            }

            //Drag and drop interface.  This can be fixed with some variables to make it more exact when selecting items.
            if (currentState.LeftButton == ButtonState.Pressed)
            {
                for (int i = 0; i < world.player.inventory.Count; i++)
                {
                    //Check position based on fontsize.
                    if (currentState.X > 500 + i * this.statsDimensions.X && currentState.X < 500 + i * this.statsDimensions.X + statsDimensions.X
                        && currentState.Y > height - 100 && currentState.Y < height - 100 + statsDimensions.Y)
                    {
                        indexSelected = i;
                        break; //Once again, unnecessary, but good for processer times.
                    }
                }
            }
            else //On release.
            {
                if (indexSelected != -1)
                {
                    //If you're over top of anothe person, (and it's not you, give them that flower).
                    if (currentSelection != null && currentSelection != world.player)
                    {
                        world.player.giveFlower(currentSelection, world.player.inventory[indexSelected]);
                    }

                    //Either way, you don't have it selected.
                    indexSelected = -1;
                }
            }

            //Button Presses
            KeyboardState currentKeyState = Keyboard.GetState();
            Keys[] currentKeysDown = currentKeyState.GetPressedKeys();
            //Run logic.
            world.player.movementX = 0;
            world.player.movementY = 0;
            foreach (Keys k in currentKeysDown)
            {
                if (k == Keys.W) { world.player.movementY = -1; }
                else if (k == Keys.S) { world.player.movementY = 1; }
                else if (k == Keys.A) { world.player.movementX = -1; }
                else if (k == Keys.D) { world.player.movementX = 1; }
            }
            

            #endregion

            #region gameLogic

            world.update();

            #endregion


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            spriteBatch.Begin();

            #region interface

            //Draw title after measuring title.
            Vector2 dimensionsTitle = titleScreen.MeasureString("Boy Meets Girl");
            spriteBatch.DrawString(titleScreen, "Boy Meets Girl", new Vector2((float)(.5 * Main.width - .5 * dimensionsTitle.X), 0), Color.Black);
            //Draw the background to the world.
            spriteBatch.Draw(colorTexture, new Rectangle(20, (int)titleDimensions.Y, width - 40, height - (int)titleDimensions.Y - 100), Color.Black);

            //Draw stats for mouseover.  Very temp as far as position goes and what font is used.  These will be moved to the bottom of the screen.
            if(currentSelection != null)
            {
                String[] toDisplay = currentSelection.getInfo(); //You'll need to do another dimension call in the future once this sprite is switched out, but that works back into margins.
                for(int i = 0; i < toDisplay.Length; i++)
                {
                    spriteBatch.DrawString(statsFont, toDisplay[i], new Vector2(25, height - 100 + (this.statsDimensions.Y - 4)*i), Color.Black);
                }
            }

            //Draw your inventory.

            for (int i = 0; i < world.player.inventory.Count; i++)
            {
                if(indexSelected != i)
                    //If the flower isn't selected.
                    spriteBatch.DrawString(statsFont, "f", new Vector2(500 + i * this.statsDimensions.X, height - 100), colors[world.player.inventory[i]]);
                else
                    //If the flower is selected.
                    spriteBatch.DrawString(statsFont, "f", new Vector2(Mouse.GetState().X, Mouse.GetState().Y), colors[world.player.inventory[i]]);
            }

            #endregion

            #region objects

            for (int i = 0; i < world.objects.Count; i++)
            {
                spriteBatch.DrawString(flowerTexture, world.objects[i].graphic, world.objects[i].position, colors[world.objects[i].color]);
            }

            #endregion

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
