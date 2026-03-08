using CommandLine;

namespace GameOfLife.Cli
{
    public class CliOptions
    {
        [Option('g', "generation", Required = false, HelpText = "Runs the simulation until the specified generation before pausing for more input.")]
        public long? RunUtilGeneration { get; set; }

        [Option('c', "columns", Required = false, HelpText = "Width of the world grid (default 400). The viewport pans across this.")]
        public int? Columns { get; set; }

        [Option('r', "rows", Required = false, HelpText = "Height of the world grid (default 200). The viewport pans across this.")]
        public int? Rows { get; set; }
    }
}
