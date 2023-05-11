using CommandLine;

namespace TSI.OCR.Tool.Utility.Misc {
    public class Options {
        [Option('u', "update", Required = true,
            HelpText = "Tell the command to automatically update OCR .apkg documents.")]
        public bool Update { get; set; }
    }
}