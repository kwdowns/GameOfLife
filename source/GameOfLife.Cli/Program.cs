using System;
using System.Runtime.InteropServices;
using System.Text;
using CommandLine;

namespace GameOfLife.Cli
{
    public class Program
    {
        // Viewport top-left corner within the world grid.
        static int _vpX = 0;
        static int _vpY = 0;

        // Usable console area (leaves room for the status bar).
        static int ViewportWidth  => Math.Max(10, Console.WindowWidth);
        static int ViewportHeight => Math.Max(5,  Console.WindowHeight - 3);

        static void Main(string[] args)
        {
            EnableAnsiOnWindows();
            Parser.Default.ParseArguments<CliOptions>(args)
                .WithParsed<CliOptions>(StartSimulationWithOptions);
        }

        // ── Rendering ────────────────────────────────────────────────────────────

        /// <summary>
        /// Redraws every visible cell from scratch (used after panning or reset).
        /// Writes one string per row to minimise flush overhead.
        /// </summary>
        public static void FullRedraw(LifeGrid grid)
        {
            int vpW = ViewportWidth;
            int vpH = ViewportHeight;
            var sb = new StringBuilder(vpW + 8);

            for (int row = 0; row < vpH; row++)
            {
                // ANSI CSI cursor-position sequence (1-indexed)
                sb.Clear();
                int gridRow = _vpY + row;
                for (int col = 0; col < vpW; col++)
                {
                    int gridCol = _vpX + col;
                    if (gridCol >= 0 && gridCol < grid.Columns &&
                        gridRow >= 0 && gridRow < grid.Rows)
                        sb.Append(grid.Grid[gridCol, gridRow] == CellState.Alive ? 'O' : '.');
                    else
                        sb.Append(' ');
                }
                Console.SetCursorPosition(0, row);
                Console.Write(sb);
            }
        }

        /// <summary>
        /// Repaints only the cells that changed between generations.
        /// Batches all writes into one ANSI string to minimise system-call overhead.
        /// </summary>
        public static void DrawDelta(LifeGrid current, LifeGrid previous)
        {
            int vpW = ViewportWidth;
            int vpH = ViewportHeight;
            var sb = new StringBuilder();
            int lastRow = -2, lastCol = -2;

            for (int row = 0; row < vpH; row++)
            {
                int gridRow = _vpY + row;
                for (int col = 0; col < vpW; col++)
                {
                    int gridCol = _vpX + col;
                    bool inBounds = gridCol >= 0 && gridCol < current.Columns &&
                                   gridRow >= 0 && gridRow < current.Rows;

                    CellState curr = inBounds ? current.Grid[gridCol, gridRow]  : CellState.Dead;
                    CellState prev = inBounds ? previous.Grid[gridCol, gridRow] : CellState.Dead;

                    if (curr == prev) continue;

                    // Only emit a cursor-move when not immediately right of the last write.
                    if (row != lastRow || col != lastCol + 1)
                        sb.Append($"\x1b[{row + 1};{col + 1}H");

                    sb.Append(curr == CellState.Alive ? 'O' : '.');
                    lastRow = row;
                    lastCol = col;
                }
            }

            if (sb.Length > 0)
                Console.Write(sb);
        }

        static void DrawStatus(LifeGrid grid, string hint = "")
        {
            int statusRow = ViewportHeight + 1;
            Console.SetCursorPosition(0, statusRow);
            var line = $"Gen {grid.Generation,6} | Pos ({_vpX},{_vpY}) | " +
                       "Arrows: pan  Space/Enter: step  g: run to gen  r: reset  q: quit";
            Console.Write(line.PadRight(Console.WindowWidth));

            if (!string.IsNullOrEmpty(hint))
            {
                Console.SetCursorPosition(0, statusRow + 1);
                Console.Write(hint.PadRight(Console.WindowWidth));
            }
        }

        // ── Simulation control ───────────────────────────────────────────────────

        public static void StartSimulationWithOptions(CliOptions options)
        {
            int rows    = options.Rows    ?? 200;
            int columns = options.Columns ?? 400;

            try
            {
                Console.SetWindowSize(
                    Math.Max(Console.WindowWidth,  80),
                    Math.Max(Console.WindowHeight, 24));
            }
            catch { /* terminal may not support resize */ }

            Console.CursorVisible = false;
            Console.Clear();

            LifeGrid grid = LifeGrid.SeedRandomGrid(columns, rows);

            // Start viewport centred on the world.
            _vpX = (columns - ViewportWidth)  / 2;
            _vpY = (rows    - ViewportHeight) / 2;

            FullRedraw(grid);
            DrawStatus(grid);

            if (options.RunUtilGeneration.HasValue)
                grid = RunSimulationUntilGeneration(grid, options.RunUtilGeneration.Value);

            do
            {
                DrawStatus(grid);
                var key = Console.ReadKey(intercept: true);

                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        Console.Clear();
                        Console.CursorVisible = true;
                        return;

                    case ConsoleKey.UpArrow:
                        _vpY = Math.Max(0, _vpY - 1);
                        FullRedraw(grid);
                        break;

                    case ConsoleKey.DownArrow:
                        _vpY = Math.Min(grid.Rows - ViewportHeight, _vpY + 1);
                        FullRedraw(grid);
                        break;

                    case ConsoleKey.LeftArrow:
                        _vpX = Math.Max(0, _vpX - 1);
                        FullRedraw(grid);
                        break;

                    case ConsoleKey.RightArrow:
                        _vpX = Math.Min(grid.Columns - ViewportWidth, _vpX + 1);
                        FullRedraw(grid);
                        break;

                    case ConsoleKey.Spacebar:
                    case ConsoleKey.Enter:
                    {
                        var prev = grid;
                        grid = grid.NextLifeGridState();
                        DrawDelta(grid, prev);
                        break;
                    }

                    case ConsoleKey.G:
                    {
                        Console.CursorVisible = true;
                        Console.SetCursorPosition(0, ViewportHeight + 2);
                        Console.Write("How many more generations to run for: ".PadRight(Console.WindowWidth));
                        Console.SetCursorPosition(0, ViewportHeight + 2);
                        Console.Write("How many more generations to run for: ");
                        var input = Console.ReadLine();
                        Console.CursorVisible = false;
                        if (long.TryParse(input, out long genCount))
                            grid = RunSimulationUntilGeneration(grid, grid.Generation + genCount);
                        FullRedraw(grid);
                        break;
                    }

                    case ConsoleKey.R:
                        grid = LifeGrid.SeedRandomGrid(columns, rows);
                        _vpX = (columns - ViewportWidth)  / 2;
                        _vpY = (rows    - ViewportHeight) / 2;
                        FullRedraw(grid);
                        break;
                }
            }
            while (true);
        }

        public static LifeGrid RunSimulationUntilGeneration(LifeGrid simulation, long targetGeneration)
        {
            LifeGrid prev = simulation;
            while (simulation.Generation < targetGeneration)
            {
                prev = simulation;
                simulation = simulation.NextLifeGridState();
                DrawDelta(simulation, prev);
                DrawStatus(simulation, $"Running… target generation {targetGeneration}");
            }
            return simulation;
        }

        // ── Platform helpers ─────────────────────────────────────────────────────

        // Enable ANSI virtual-terminal processing on Windows so escape sequences work.
        static void EnableAnsiOnWindows()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            try
            {
                var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
                if (GetConsoleMode(handle, out uint mode))
                    SetConsoleMode(handle, mode | 0x0004u); // ENABLE_VIRTUAL_TERMINAL_PROCESSING
            }
            catch { /* best-effort; modern Windows Terminal enables this automatically */ }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    }
}
