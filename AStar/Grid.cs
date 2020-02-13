using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AStarMonoGame
{
    class Grid : GameObject, IEnumerable<GridCell>

    { 
        /// <summary>
        /// The cells of the grid
        /// </summary>
        private GridCell[,] cells;

        /// <summary>
        /// The cell size in pixels
        /// </summary>
        public int CellSize { get; private set; }

        public GridCell Source { get; set; }
        public GridCell Destination { get; set; }

        /// <summary>
        /// The path, if one exists, as a list of cells 
        /// from the souce cell to the destination cell
        /// </summary>
        public IEnumerable<GridCell> Path { get; private set; }


        public Grid(Game game, int cellSize)   : base(game)
        {
            Color lineColor = Color.Black;

            CellSize = cellSize;

            pixel = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);

            pixel.SetData(new[] { lineColor });

            InitialiseCells();
        }



        public GridCell CellAtCoordinate(float x, float y)
        {
            var position = PositionAtCoordinate(x, y);
            return CellAtPosition(position.X, position.Y);
        }

        /// <summary>
        /// Returns a cell at position (x, y) on the grid
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public GridCell CellAtPosition(int x, int y)
        {
            GridCell cell;

            cell = null; 

            if (IsPositionValid(x, y))
            {
                cell = cells[x, y];

            }
            return cell;
        }


        private bool IsPositionValid(int x, int y)
        {
            return x >= 0 && x< GridWidth && y >= 0 && y < GridHeight;
        }



        private IEnumerable<GridCell> GetAdjacentCells(GridCell centreCell)
        {
            GridCell adjacentCell;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    adjacentCell = CellAtPosition(centreCell.Position.X + i, centreCell.Position.Y + j);

                    // if the adjacentCell is not null and the adjacentCell is not the centreCell 
                    if (adjacentCell != null && adjacentCell != centreCell)
                    {
                        yield return adjacentCell;
                    }
                }
            }
        }

        public List<GridCell> GetValidAdjacentCells(GridCell cell)
        {
          // get all the adjacent cells
            IEnumerable<GridCell> allAdjacentCells = GetAdjacentCells(cell);

            List<GridCell> validCells = new List<GridCell>();


            // now go through each cell 
            
            foreach (GridCell c in allAdjacentCells)
            {
                // to see if it is walkable to from the current cell
                if (CellIsWalkableTo(cell, c))
                {
                    //and is not on the clased list and itself is not an unwalkable cell
                    if (c.State != GridCellState.Closed && c.Type != GridCellType.Unwalkable)
                    {
                        validCells.Add(c);
                    }
                }
            }

            return validCells;
        }
        /// <summary>
        /// Returns true if a cell is walkable
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public bool CellIsWalkableTo(GridCell source, GridCell destination)
        {
            return !(CellAtPosition(source.Position.X, destination.Position.Y).Type == GridCellType.Unwalkable || CellAtPosition(destination.Position.X, source.Position.Y).Type == GridCellType.Unwalkable);
        }

        public void Reset()
        {
            foreach (GridCell cell in this)
            {
                cell.Reset();
            }
        }

        public void Clear()
        {
            foreach (GridCell cell in this)
            {
                cell.Clear();
            }
        }

        public void Resize(int cellSize)
        {
            CellSize = cellSize;
            InitialiseCells();
            //Clear();
        }


      private  Point PositionAtCoordinate(float x, float y)
        {
            return new Point((int)Math.Ceiling(Convert.ToDouble(GridWidth * x / ScreenWidth)) - 1, (int)Math.Ceiling(Convert.ToDouble(GridHeight * y / ScreenHeight)) - 1);
        }



      private void newCell_TypeChanged(object sender, GridCellTypeChangedEventArgs e)
        {
            GridCell  cell = (GridCell)sender;

            if (e.NewType == GridCellType.Source)
            {
                if (Source != null && Source != cell)
                {
                    Source.Type = GridCellType.Walkable;
                }
                Source = cell;
            }
            else if (e.NewType == GridCellType.Destination)
            {
                if (Destination != null && Destination != cell)
                {
                    Destination.Type = GridCellType.Walkable;
                }
                Destination = cell;
            }
        }



      private  void InitialiseCells()
        {

            GridCell newCell;

            cells = new GridCell[GridWidth, GridHeight];


           for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
                {
                    newCell = new GridCell(Game, GridCellType.Walkable, new Point(i, j), new Point(i * CellSize, j * CellSize), CellSize, CellSize);
                    cells[i, j] = newCell;


                    newCell.TypeChanged += newCell_TypeChanged;
                }
            }
        }
        public IEnumerator<GridCell> GetEnumerator()
        {
            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
                {
                    yield return cells[i, j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        //======================================================================
        //Drawing methods, fields and properties

        private Texture2D pixel;
        public bool DrawGridLines { get; set; }

        public override void Draw(GameTime gameTime)
        {
            DrawCells(gameTime);
            if (DrawGridLines)
            {
                DrawLines();
            }
        }

        private void DrawCells(GameTime gameTime)
        {
            foreach (GridCell cell in cells)
            {
                cell.Draw(gameTime);
            }
        }



        private void DrawLines()
        {
            for (int i = 1; i < GridWidth; i++)
            {
                SpriteBatch.Draw(pixel, new Rectangle(i * CellSize, 0, 1, Game.Window.ClientBounds.Height), Color.White);
            }
            for (int i = 1; i < GridHeight; i++)
            {
                SpriteBatch.Draw(pixel, new Rectangle(0, i * CellSize, Game.Window.ClientBounds.Width, 1), Color.White);
            }
        }
        //==================================================================


         /// <summary>
        /// returns the width of the grid in number cells
        /// </summary>
        public int GridWidth
        {
            get
            {
                return ScreenWidth/CellSize;
            }
        }

        public int GridHeight
        {
            get
            {
                return ScreenHeight/CellSize;
            }
        }
    }
}