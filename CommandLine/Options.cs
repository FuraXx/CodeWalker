using System.Collections.Generic;
using CodeWalker.CommandLine.Arguments;

namespace CodeWalker.CommandLine
{
    [Verb("menu", HelpText = "Open CodeWalker in menu mode")]
    public class MenuOptions
    {

    }

    [Verb("explorer", HelpText = "Open CodeWalker in explorer mode")]
    public class ExploreOptions
    {

    }

    [Verb("world", HelpText = "Open CodeWalker in world mode (default)")]
    public class WorldOptions
    {

    }

    [Verb("meta", HelpText = "[commandline] Open CodeWalker in meta mode")]
    public class MetaOptions
    {
        [Option("import", Default = false, HelpText = "Import meta xml files")]
        public bool Import { get; set; }

        [Option("export", Default = false, HelpText = "Export meta files to xml")]
        public bool Export { get; set; }

        [Option("genstructs", Default = false, HelpText = "Generate C# structs")]
        public bool GenStructs { get; set; }

        [Option('i', "input", HelpText = "Input file(s)")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('o', "output", HelpText = "Output directory")]
        public string OutputDirectory { get; set; }

        [Option("dlc", Default = false, HelpText = "Load latest DLC")]
        public bool EnableDlc { get; set; }

    }

    [Verb("rpf", HelpText = "[commandline] Open CodeWalker in RPF mode")]
    public class RpfOptions
    {
        [Option("extract", Default = false, HelpText = "Extract files")]
        public bool Extract { get; set; }

        [Option('i', "input", HelpText = "Input file(s)")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('o', "output", HelpText = "Output directory")]
        public string OutputDirectory { get; set; }

        [Option("dlc", Default = false, HelpText = "Load latest DLC")]
        public bool EnableDlc { get; set; }
    }

    [Verb("rsc7", HelpText = "[commandline] Open CodeWalker in RSC7 mode")]
    public class Rsc7Options
    {
        [Option("inflate", Default = false, HelpText = "Inflate files")]
        public bool Inflate { get; set; }

        [Option("deflate", Default = false, HelpText = "Deflate files")]
        public bool Deflate { get; set; }

        [Option('i', "input", HelpText = "Input file(s)")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('o', "output", HelpText = "Output directory")]
        public string OutputDirectory { get; set; }

        [Option("dlc", Default = false, HelpText = "Load latest DLC")]
        public bool EnableDlc { get; set; }
    }
}
