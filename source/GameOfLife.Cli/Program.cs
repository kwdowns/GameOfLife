using System;
using CommandLine;

namespace GameOfLife.Cli
{
    public class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CliOptions>(args)
                .WithParsed<CliOptions>(StartSimulationWithOptions);            
        }

        public static void Print(LifeGrid lifeGrid, string message = "")
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
            Console.WriteLine(message);

            
        }

        public static void StartSimulationWithOptions(CliOptions options)
        {
            var rows = options.Rows??30;
            var columns = options.Columns??60;
            Console.SetWindowSize(Math.Max(Console.WindowWidth, columns + 2), Math.Max(Console.WindowHeight, rows + 4));
            Console.CursorVisible = false;
            Console.Clear();

            LifeGrid grid = LifeGrid.SeedRandomGrid(columns, rows);
            Print(grid);

            if(options.RunUtilGeneration.HasValue)
                grid = RunSimulationUntilGeneration(grid, options.RunUtilGeneration.Value);

            do{
                Print(grid, "Press 'q' to quit.\r\nPress 'g' to specify the next generation to stop at.\r\nPress 'r' to start a new simulation.\r\nAll other keys advance the simulation one generation.");
                var key = Console.ReadKey();           
            
                if(key.KeyChar == 'q')
                    return;
                if(key.KeyChar == 'g')
                {
                    Console.Write("\r\nHow many more generations to run for: ");
                    var generationsInput = Console.ReadLine();
                    var generation = long.Parse(generationsInput);
                    Console.Clear();
                    grid = RunSimulationUntilGeneration(grid, grid.Generation + generation);
                }
                if(key.KeyChar == 'r')
                {
                    grid = LifeGrid.SeedRandomGrid(columns, rows);
                }
            }while(true);
        }


        public static LifeGrid RunSimulationUntilGeneration(LifeGrid simulation, long generation)
        {
            while(simulation.Generation < generation)
            {

                simulation = simulation.NextLifeGridState();
                Print(simulation);
            }
            return simulation;
        }
    }
}
