using System;

namespace GameOfLife.Cli
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 50);
            Console.CursorVisible = false;
            Console.Clear();
            LifeGrid grid = LifeGrid.SeedRandomGrid(80,40);

            Print(grid);

            do{
                grid = grid.NextLifeGridState();
                var key = Print(grid);
                if(key.KeyChar == 'q')
                    return;
            }while(grid.Generation < 3000);
        }

        public static ConsoleKeyInfo Print(LifeGrid lifeGrid)
        {
            for(int row = 0; row < lifeGrid.Rows; row++)
            {
                for(int column = 0; column < lifeGrid.Columns; column++)
                {
                    char write = '.';
                    Console.SetCursorPosition(column,row);
                    var cellState = lifeGrid.Grid[column,row];
                    if(cellState == CellState.Alive)
                        write = 'O';
                    Console.Write(write);                    
                }
            }
            Console.SetCursorPosition(0, lifeGrid.Rows + 2);
            Console.WriteLine("Generation: " + lifeGrid.Generation.ToString());
            var key = Console.ReadKey();
            return key;
        }
    }
}
