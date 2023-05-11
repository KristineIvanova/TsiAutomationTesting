using CommandLine;
using Serilog;
using TSI.OCR.Tool.Utility.Commands;
using TSI.OCR.Tool.Utility.Misc;

namespace TSI.OCR.Tool.Utility {
    class Program {
        static async Task Main(string[] args) {
            var config = new LoggerConfiguration()
                .WriteTo.Console();

            var logger = config.CreateLogger().ForContext(typeof(Program));

            await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(async o => {
                    if (o.Update) {
                        var up = new ApkgUpdater(logger);
                        await up.Update();
                    }
                });
        }
    }
}