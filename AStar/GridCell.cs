using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AStarMonoGame
{
    /// <summary>
    /// The set of cell types
    /// Add to set to create new cell types
    /// </summary>
    enum GridCellType
    {
        Walkable,
        Unwalkable,
        Source,
        Destination,
        Path,
        ThickMud,
        ApatureSpeedGoop
    }
     
    /// <summary>
    /// Used during the search process to move a gridcell to 
    /// the opne or closed lists
    /// </summary>
    enum GridCellState
    {
        Open,
        Closed,
        NotVisited
    }

    class GridCell:GameObject
    {
        /// <summary>
        ///  the type of this gridcell
        /// </summary>
        private GridCellType type;

        /// <summary>
        /// The state of this gridcell
        /// </summary>
        public GridCellState State { get; set; }

        /// <summary>
        /// Parent of this grid cell
        /// Uesed when generating a list 
        /// of cells from source to destination
        /// </summary>
        public GridCell Parent { get; set; }


        /// <summary>
        /// the A* values associated with this cell
        /// </summary>
        public int G { get; set; }
        public double H { get; set; }
        public double F { get; set; }


        //=================================================
        // properties used for coordinates in drawing
        // not used in A* algorithm

        public Point Position { get; private set; }
        public Point ScreenCoordinates { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public event EventHandler<GridCellTypeChangedEventArgs> TypeChanged = delegate { };

        private Texture2D pixel;
        //=================================================

        public GridCell(Game game, GridCellType type, Point position, Point screenCoordinates, int width, int height) : base(game)
        {
            Type = type;
            Position = position;
            ScreenCoordinates = screenCoordinates;
            Width = width;
            Height = height;
            pixel = new Texture2D(game.GraphicsDevice,1,1,true,SurfaceFormat.Color);
            pixel.SetData(new[] {Color.White});
            State = GridCellState.NotVisited;
        }


        /// <summary>
        /// Draws this grid cell
        /// If enum GridCellType are added to then ammend this method
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            Color colour;

            colour = Color.Gray;

            if (Type == GridCellType.Path)
            {
                colour = Color.Yellow;
            }
            else if (Type == GridCellType.Unwalkable)
            {
                colour = Color.Black;
            }
            else if (Type == GridCellType.Source)
            {
                colour = Color.Green;
            }
            else if (Type == GridCellType.Destination)
            {
                colour = Color.Red;
            }
            else if (State == GridCellState.Closed)
            {
                colour = Color.LightSkyBlue;
            }
            else if (State == GridCellState.Open)
            {
                colour = Color.DarkSlateBlue;
            }
            else if (Type == GridCellType.Walkable)
            {
                colour = Color.Gray;
            }
            else if(Type == GridCellType.ThickMud)
            {
                colour = Color.SaddleBrown;
            }
            else if(Type == GridCellType.ApatureSpeedGoop)
            {
                colour = Color.Orange;
            }
            else 
            {
                colour = Color.CornflowerBlue;
            }

            SpriteBatch.Draw(pixel, new Rectangle(ScreenCoordinates.X, ScreenCoordinates.Y, Width, Height), colour);
        }

        public void Reset ()
        {
            if (Type == GridCellType.Path)
            {
                type = GridCellType.Walkable;
            }
            State = GridCellState.NotVisited;
            Parent = null;
            H = G = 0;
        }

        public void Clear()
        {
            Reset();
            Type = GridCellType.Walkable;
        }



        /// <summary>
        /// This is called when a solution is found
        /// To find a path, trace back from this cell and following
        /// this cells parents until there is no parent, i.e. when
        /// the parent is null.
        /// 
        /// Then reverse the path
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GridCell> GetPath()
        {
            List<GridCell> path = new List<GridCell>();

            // start at this cell
            GridCell currCell = this;

            
            while (currCell != null)
            {

                // assign GridCellType=Path for drawing purposes only
                // but omit the source and destination cells
                if (currCell.Type == GridCellType.Walkable)
                {
                    currCell.Type = GridCellType.Path;
                }

                // this is where the actual path is built.
                path.Add(currCell);

                currCell = currCell.Parent;
            }

            path.Reverse();

            path.RemoveAt(0); //remove the grid source

            return path;
        }

        public GridCellType Type
        {
            get
            {
                return type;
            }
            set
            {
                GridCellType oldType = type;
                type = value;
                TypeChanged(this, new GridCellTypeChangedEventArgs(oldType, value));
            }
        }


        public bool IsOrthagonalWith(GridCell otherCell)
        {
            return Position.X == otherCell.Position.X || Position.Y == otherCell.Position.Y;
        }

        public override string ToString()
        {
            return String.Format("Position: {0} \t ScreenCoordinates {1} ", Position, ScreenCoordinates);
        }

    }
}
