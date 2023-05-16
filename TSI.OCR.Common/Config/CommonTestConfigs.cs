using Microsoft.Extensions.Configuration;

namespace TSI.OCR.Common.Config {
    public static class CommonTestConfigs {
        private static string solutionPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
        private static string pathToNewApkgFiles = Path.GetTempPath();

        public static string pathToRootDirectoryWithFiles = Path.Combine(solutionPath, "TSI.OCR.Auto.Tests", "Resource", "Documents");


        public static string errorImagePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "SourceFile", "Crop Exception.png");
        public static string PathToNewApkgFiles = Path.GetTempPath();
        
        public static string HtmlLogFolder { get; }
        public static string ApkgDebugFolder { get; }
        public static string ColorMistakeBegin { get; }
        public static string ColorMistakeEnd { get; }
        public static int CoordinatesRange { get; }
        public static string ApiPostFile { get; }
        public static string ApiGetFile { get; }
        public static string MongoDbRestoreExe { get; }
        public static string AlphaAutoMongoDbAddress { get; }
        public static string MongoDumpFile { get; }
        
        static CommonTestConfigs() {
            // check global Environment
            var configFileName = "";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(solutionPath, "TSI.OCR.Common", "Config"))
                //.AddJsonFile(configFileName, false, false)
                .Build();

            HtmlLogFolder = configuration.GetSection("HTML_LOG_FOLDER").Value;
            ApkgDebugFolder = configuration.GetSection("APKG_DEBUG_FOLDER").Value;
            ColorMistakeBegin = configuration.GetSection("COLOR_MISTAKE_BEGIN").Value;
            ColorMistakeEnd = configuration.GetSection("COLOR_MISTAKE_END").Value;
            var value = configuration.GetSection("COORDINATES_RANGE").Value;
            if (value != null)
            {
                CoordinatesRange = int.Parse(value);
                ApiPostFile = configuration.GetSection("API_POST_FILE").Value;
                ApiGetFile = configuration.GetSection("API_GET_FILE").Value;
                MongoDbRestoreExe = configuration.GetSection("MONGO_DB_RESTORE_EXE").Value;
                AlphaAutoMongoDbAddress = configuration.GetSection("ALPHA_AUTO_MONGO_DB_ADDRESS").Value;
                MongoDumpFile = configuration.GetSection("MONGO_DUMP_FILE").Value;
            }
        }
    }
}