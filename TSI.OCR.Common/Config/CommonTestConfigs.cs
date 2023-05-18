using Microsoft.Extensions.Configuration;

namespace TSI.OCR.Common.Config
{
    public static class CommonTestConfigs
    {
        private static string solutionPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
        public static string pathToRootDirectoryWithFiles = Path.Combine(solutionPath, "TSI.OCR.Auto.Tests", "Resource", "Documents");

        public static string errorImagePath =
            Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "SourceFile", "Crop Exception.png");

        public static string PathToNewFiles = Path.GetTempPath();


        public static string HtmlLogFolder { get; }
        public static string DebugFolder { get; }
        public static string ColorMistakeBegin { get; }
        public static string ColorMistakeEnd { get; }
        public static int CoordinatesRange { get; }
        public static string ApiPostFile { get; }
        public static string ApiGetFile { get; }

        static CommonTestConfigs()
        {
            Environment.SetEnvironmentVariable("TEST_CONFIG_FILE", "Windows.config");

            // check global Environment
            var configFileName = "";
            var testConfigFile = Environment.GetEnvironmentVariable("TEST_CONFIG_FILE");


            if (testConfigFile != null)
            {
                configFileName = testConfigFile;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(solutionPath, "TSI.OCR.Common", "Config"))
                .AddJsonFile(configFileName, false, false)
                .Build();


            HtmlLogFolder = configuration.GetSection("HTML_LOG_FOLDER").Value;
            DebugFolder = configuration.GetSection("DEBUG_FOLDER").Value;
            ColorMistakeBegin = configuration.GetSection("COLOR_MISTAKE_BEGIN").Value;
            ColorMistakeEnd = configuration.GetSection("COLOR_MISTAKE_END").Value;
        }
    }
}