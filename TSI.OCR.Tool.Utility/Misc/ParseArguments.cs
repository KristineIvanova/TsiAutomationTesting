using CommandLine;

namespace TSI.OCR.Main.Misc
{
    public class Options
    {
        [Option('u', "update", Required = true,
            HelpText = "Tell the command to automatically update OCR  documents")]
        public bool Update { get; set; }
    }
}