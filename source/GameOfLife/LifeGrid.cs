using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class LifeGrid
    {
        private LifeGrid(int columns, int rows, CellState[,] initalGrid)
        {
            Grid = initalGrid;
            SeedStateGrid = initalGrid;
            Columns = columns;
            Rows = rows;
            Generation = 0;
        }

        private LifeGrid(LifeGrid previousState, CellState[,] nextState)
        {
            Columns = previousState.Columns;
            Rows = previousState.Rows;
            Generation = previousState.Generation + 1;
            Grid = nextState;
        }

        public LifeGrid NextLifeGridState()
        {
            var nextGrid = new CellState[Columns, Rows];
            Parallel.For(0, Columns, i =>
            {
                for (int j = 0; j < Rows; j++)
                {
                    nextGrid[i, j] = NextCellState(Grid[i,j], GetNumberOfAliveNeighbors(i, j));
                }
            });

            return new LifeGrid(this, nextGrid);
        }

        public static CellState NextCellState(CellState previousState, int aliveNeighbors)
        {
            Func<CellState, int, bool> isLonely = (p,c) => p.Equals(CellState.Alive) && c < 2;
            Func<CellState, int, bool> isHealthy = (p,c) => p.Equals(CellState.Alive) && c == 2 || c == 3;
            Func<CellState, int, bool> isCrowded = (p,c) => p.Equals(CellState.Alive) && c > 3;
            Func<CellState, int, bool> isReproducing = (p,c) => p.Equals(CellState.Dead) && c == 3;

            // this is where i wish c# supported discriminated unions
            switch (aliveNeighbors)
            {
                case var aN when isLonely(previousState, aN):
                    return CellState.Dead;
                case var aN when isHealthy(previousState, aN):
                    return CellState.Alive;
                case var aN when isCrowded(previousState, aN):
                    return CellState.Dead;
                case var aN when isReproducing(previousState, aN):
                    return CellState.Alive;
                default:
                    return CellState.Dead;
            }
        }

        public static (int column, int row) MapCoordsToTorus((int column, int row) coords, int columns, int rows)
        {
            var column = coords.column;
            var row = coords.row;

            var spillsOverTopEdge = (row < 0);
            var spillsOverBottomEdge = (row == rows);

            var spillsOverLeftEdge = (column < 0);
            var spillsOverRightEdge = (column == columns);

            var mappedRow = spillsOverTopEdge ? rows - 1
                                : spillsOverBottomEdge ? 0
                                : row;

            var mappedColumn = spillsOverLeftEdge ? columns - 1
                                : spillsOverRightEdge ? 0
                                : column;

            return (mappedColumn, mappedRow);
        }

        public int GetNumberOfAliveNeighbors(int column, int row)
        {
            //our life grid will wrap around
            Func<(int, int), bool> cellStateAtMappedCoordsIsAlive = (cr) =>
              {
                  return Grid[cr.Item1, cr.Item2] == CellState.Alive;
              };

            /*
            (0,0)(1,0)(2,0)
            (0,1)(1,1)(2,2)
            (0,2)(1,2)(2,2)
            */

            var neighborCellCoords = new (int, int)[]{
                (column - 1, row - 1), //northwest
                (column, row - 1), //north
                (column + 1, row - 1), //northeast
                (column - 1, row), //west
                (column + 1, row), //east
                (column - 1, row + 1), //southwest
                (column, row + 1), // south
                (column + 1, row + 1) //southeast
            };

            return neighborCellCoords
                .Select(c => MapCoordsToTorus(c, Columns, Rows))
                .Where(cords => Grid[cords.column, cords.row] == CellState.Alive)
                .Count();
        }

        public static LifeGrid SeedRandomGrid(int columns, int rows)
        {
            //get random states
            var bufferSize = (int)Math.Ceiling((decimal)(columns * rows) / 8m);
            var buffer = new byte[bufferSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buffer);
            }

            //set up our random cell states
            Stack<CellState> states = new Stack<CellState>();
            var cellStateSequence = buffer.SelectMany(b =>
                Convert.ToString(b, 2).PadLeft(8, '0')
                .Select(c => c == '1' ? CellState.Alive : CellState.Dead)
            );
            foreach (var state in cellStateSequence)
                states.Push(state);

            // assign the random cell states to the grid
            var seedStateGrid = new CellState[columns, rows];
            for (int i = 0; i < columns; i++)
                for (int j = 0; j < rows; j++)
                {
                    seedStateGrid[i, j] = states.Pop();
                }

            return new LifeGrid(columns, rows, seedStateGrid);
        }

        public int Columns { get; }
        public int Rows { get; }
        public long Generation { get; }
        public CellState[,] Grid { get; }

        public CellState[,] SeedStateGrid { get; }
    }
}
