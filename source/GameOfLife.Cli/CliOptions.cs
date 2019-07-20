using CommandLine;

namespace GameOfLife.Cli
{
    public class CliOptions
    {
        [Option('g', "generation", Required = false, HelpText = "Runs the simulation until the specified generation before pausing for more input.")]
        public long? RunUtilGeneration { get; set; }

        [Option('c', "columns", Required = false, HelpText = "Number of columns in the simulation, defaults to 60.")]
        public int? Columns { get; set; }

        [Option('r', "rows", Required = false, HelpText = "Number of rows in the simulation, defaults to 30.")]
        public int? Rows { get; set; }
    }
}
