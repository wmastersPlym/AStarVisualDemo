using AStarMonoGame;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace AStarMonoGame
{

    public class AStarExample : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private List<GameObject> gameObjects;

        private Grid grid;
        private AStar aStar;
        private int gridSize;

        // Input fields
        private Texture2D mouseCursorTexture;
        private Vector2 mouseCursorPosition;
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;

        private SpriteFont messageFont;
        private Vector2 messagePosition;


        public AStarExample()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            gameObjects = new List<GameObject>();

            mouseCursorPosition = new Vector2(0, 0);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            messagePosition.X = 30;
            messagePosition.Y = 30;

            gridSize = 20;

            grid = new Grid(this, gridSize) { DrawGridLines = true };

            aStar = new AStar(this, grid, AStar.AStarHeuristic.Manhatten);

            gameObjects.Add(grid);
            gameObjects.Add(aStar);


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
          //  spriteBatch = new SpriteBatch(GraphicsDevice);

           messageFont = Content.Load<SpriteFont>("fonts\\MessageFont");
           mouseCursorTexture = Content.Load<Texture2D>("images\\cursor");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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

#if WINDOWS
            HandleKeyboardEvents();
            HandleMouseEvents();
#endif

            aStar.Update(gameTime);

            base.Update(gameTime);
        }

        private IEnumerable<GridCell> SolutionPath;
        private bool isPathWritten;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            String message;

            message = "heuristic: " + aStar.GetHeuristicAsString();
            message += "\n # Visted nodes = " + aStar.TotalVisited;

            if (aStar.PathFound)
            {
                message += "\n Path length = " + aStar.PathLength;
                SolutionPath = aStar.SolutionPath;

                // used to display the path to the console window just once
                if (isPathWritten == false)
                {
                    Console.WriteLine(" Solution path: ================== ");
                    foreach (GridCell cell in SolutionPath)
                    {
                        Console.WriteLine(cell.ToString());
                      
                    }
                    isPathWritten = true;
                }
            }

            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            grid.Draw(gameTime);
            aStar.Draw(gameTime);

            spriteBatch.Draw(mouseCursorTexture, mouseCursorPosition, Color.White);
            spriteBatch.DrawString(messageFont, message, messagePosition, Color.Yellow);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void HandleKeyboardEvents()
        {
            // remember the last key pressed
            previousKeyboardState = currentKeyboardState;

            currentKeyboardState = Keyboard.GetState();

            // exit when escape key is pressed
            if (currentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            if (currentKeyboardState.IsKeyDown(Keys.Enter))
            {
                isPathWritten = false;
                grid.Reset();
                aStar.Start();
            }


            if (currentKeyboardState.IsKeyDown(Keys.Space) && !previousKeyboardState.IsKeyDown(Keys.Space))
            {
                grid.DrawGridLines = !grid.DrawGridLines;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Q))
                aStar.CurrentHeuristic = AStar.AStarHeuristic.Manhatten;

            else if (currentKeyboardState.IsKeyDown(Keys.W))
                aStar.CurrentHeuristic = AStar.AStarHeuristic.Diagonal;

            else if (currentKeyboardState.IsKeyDown(Keys.E))
                aStar.CurrentHeuristic = AStar.AStarHeuristic.Euclidean;

            else if (currentKeyboardState.IsKeyDown(Keys.R))
                aStar.CurrentHeuristic = AStar.AStarHeuristic.Dijkstra;
        }

        private void SetCellType(GridCell cell, GridCellType type)
        {
            if (cell == null)
            {
                return;
            }
            cell.Type = type;
        }

        private void HandleMouseEvents()
        {
            MouseState ms = Mouse.GetState();
            Point curMousePos;

            curMousePos.X = ms.X;
            curMousePos.Y = ms.Y;

            mouseCursorPosition.X = ms.X;
            mouseCursorPosition.Y = ms.Y;


            if (ms.LeftButton == ButtonState.Pressed)
            {
                HandleLeftButton(curMousePos);
            }
            if (ms.RightButton == ButtonState.Pressed)
            {

                HandleRightButton(curMousePos);
            }
        }

        private void HandleLeftButton(Point position)
        {
            var cell = grid.CellAtCoordinate(position.X, position.Y);
            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.LeftControl))
            {
                SetCellType(cell, GridCellType.Source);
            }
            else if (state.IsKeyDown(Keys.LeftAlt))
            {
                SetCellType(cell, GridCellType.Destination);
            }
            else if(state.IsKeyDown(Keys.A))
            {
                SetCellType(cell, GridCellType.ThickMud);
            }
            else if (state.IsKeyDown(Keys.S))
            {
                SetCellType(cell, GridCellType.ApatureSpeedGoop);
            }
            else
            {
                SetCellType(cell, GridCellType.Unwalkable);
            }
        }

        private void HandleRightButton(Point position)
        {
            GridCell cell = grid.CellAtCoordinate(position.X, position.Y);
            SetCellType(cell, GridCellType.Walkable);
        }
    }
}
