using System.Threading.Tasks;
using CommandLine;
using Serilog;
using TSI.OCR.Main.Commands;
using TSI.OCR.Main.Misc;

namespace TSI.OCR.Main
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new LoggerConfiguration()
                .WriteTo.Console();

            var logger = config.CreateLogger().ForContext(typeof(Program));

            await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(async o =>
                {
                    if (o.Update)
                    {
                        var up = new ReferenceDatabaseUpdater(logger);
                        await up.Update();
                    }
                });
        }
    }
}