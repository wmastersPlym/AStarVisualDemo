using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace AStarMonoGame
{
    class PathFoundEventArgs : EventArgs
    {
        public IEnumerable<GridCell> Path { get; set; }

        public int TotalPathLength { get; set; }
        public int TotalExplored { get; set; }


        public PathFoundEventArgs(IEnumerable<GridCell> path, int pathLength, int totalExplored)
        {
            Path = path;
            TotalPathLength = pathLength;
            TotalExplored = totalExplored;
        }
    }


    class AStar : GameObject
    {

        public enum AStarHeuristic { Manhatten, Euclidean, Diagonal, Dijkstra };

        public AStarHeuristic CurrentHeuristic  {set; get;}


        public bool IsActive { get; set; }
        public event EventHandler<PathFoundEventArgs> PathFoundEvent = delegate { };

        private BinaryHeap<double, GridCell> openList;
        private Grid grid;
        private GridCell currentCell;

        public IEnumerable<GridCell> SolutionPath {private set; get;}

        public int TotalVisited {private set; get;}
        public bool PathFound { private set; get; }
        public int PathLength {private set; get; }

        public AStar(Game game, Grid grid, AStarHeuristic heuristic)
            : base(game)
        {
            this.grid = grid;
            CurrentHeuristic = heuristic;
            IsActive = false;
           PathFound = false;
        }

        public void Start()
        {
            if (grid.Source == null || grid.Destination == null)
            {
                return;
            }
            TotalVisited = 0;
            IsActive = true;

            // initialise the BinaryHeap
            openList = new BinaryHeap<double, GridCell>();

            // set the current cell to be the starting cell
            currentCell = grid.Source;

            // define the current cell to be closed
            currentCell.State = GridCellState.Closed;

            PathFound = false;

            // only call FindPath() if not interested
            // in seeing the path being built.
           // FindPath();
        }

        private void FindPath()
        {

            if (!IsActive)
            {
                return;
            }

            while (IsActive == true)
            {
                PathOneStep();
            }
        }

        /// <summary>
        /// Each time through the game loop add one step 
        /// along the path. This gives a slower dispaly which 
        /// shows the path being built
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {

            if (!IsActive)
            {
                return;
            }

            PathOneStep();

            base.Update(gameTime);
        }


        private void PathOneStep()
        {
            GridCellType cellType;
            int gCost;

            foreach (GridCell currentAdjacentCell in grid.GetValidAdjacentCells(currentCell))
            {
                if (currentAdjacentCell.State != GridCellState.Open)
                {
                    currentAdjacentCell.State = GridCellState.Open;

                    currentAdjacentCell.Parent = currentCell;

                    currentAdjacentCell.H = GetHeuristricEstimate(currentAdjacentCell.Position, grid.Destination.Position) * 10;

                    currentAdjacentCell.H += GetTieBreaker(currentAdjacentCell);

                    // to be used with differing terain costs
                    cellType = currentAdjacentCell.Type;


                    // Thick mud is hard to walk through
                    if(cellType == GridCellType.ThickMud)
                    {
                        currentAdjacentCell.G += 100;
                    } else if(cellType == GridCellType.ApatureSpeedGoop)
                    {
                        currentAdjacentCell.G += 5;
                    }

                    if (cellType == GridCellType.Walkable)
                    {
                        if (currentCell.IsOrthagonalWith(currentAdjacentCell))
                        {
                            gCost = 10;
                        }
                        else
                        {
                            gCost = 14;
                        }
                        currentAdjacentCell.G = currentCell.G + gCost;
                    }


                    // determine the final f(n) value for this node
                    currentAdjacentCell.F = currentAdjacentCell.G + currentAdjacentCell.H;

                    openList.Insert(currentAdjacentCell.F, currentAdjacentCell);
                }
            }  // end foreach

            if (openList.Count == 0) //A path cannot be found
            {
                IsActive = false;
                return;
            }
            TotalVisited++;

            // get the currentCell to be the minimium on the openList
            currentCell = openList.RemoveMin();

            // set the state of currentcell to be closed
            // this is the same as putting it on the closed list
            currentCell.State = GridCellState.Closed;

            // if the current cell is the destination
            // then build the solution path
            if (currentCell.Type == GridCellType.Destination)
            {
                IsActive = false;

                BuildSolutionPath();
            }

        }

        private void BuildSolutionPath()
        {
            SolutionPath = currentCell.GetPath();

            TotalVisited = TotalVisited - 1;

            PathFound = true;

            PathLength = SolutionPath.Count();
        }


        private int GetHeuristricEstimate(Point source, Point destination)
        {
            int estimate = 0;

            if (CurrentHeuristic == AStarHeuristic.Manhatten)  // Manhatten Distance

                estimate = (int)Math.Abs(destination.X - source.X) + Math.Abs(destination.Y - source.Y);

            else if (CurrentHeuristic == AStarHeuristic.Diagonal)  // Diagonal Distance

                estimate = (int)Math.Max(Math.Abs(destination.X - source.X), Math.Abs(destination.Y - source.Y));

            else if (CurrentHeuristic == AStarHeuristic.Euclidean)  // Euclidean Distance

                estimate = (int)Math.Sqrt(Math.Pow(source.X - destination.X, 2) + Math.Pow(source.Y - destination.Y, 2));

            else if (CurrentHeuristic == AStarHeuristic.Dijkstra)  // Dijkstra

                estimate = 0;
        
            return estimate;
        }

     private   double GetTieBreaker(GridCell cell)
        {
         /*
          * * http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html#S12
          * */
         int dx1 = cell.Position.X - grid.Destination.Position.X;
         int dy1 = cell.Position.Y - grid.Destination.Position.Y;
         int dx2 = grid.Source.Position.X - grid.Destination.Position.X;
         int dy2 = grid.Source.Position.Y - grid.Destination.Position.Y;
         int  cross = Math.Abs(dx1 * dy2 - dx2 * dy1);

          return cross * 0.001;
        }

     public string GetHeuristicAsString()
     {
         string heuristicAsString = "";

         if (CurrentHeuristic == AStarHeuristic.Manhatten)  // Manhatten Distance
         {
             heuristicAsString = " Manhatten";
         }
         else if (CurrentHeuristic == AStarHeuristic.Diagonal)  // Diagonal Distance
         {
             heuristicAsString = " Diagonal";
         }
         else if (CurrentHeuristic == AStarHeuristic.Euclidean)  // Euclidean Distance
         {
             heuristicAsString = " Euclidean";
         }
         else if (CurrentHeuristic == AStarHeuristic.Dijkstra)  // Dijkstra
         {
             heuristicAsString = " Dijkstra";
         }

         return heuristicAsString;
     }
    }
}
