using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TSI.OCR.Auto.Tests.Misc.JsonConvertObject {
    
    public static class JsonConvertService {
        public static string GetFeatureFlagsFromFile(string jSonFilePath) {
            if (File.Exists(jSonFilePath) == false) {
                return null;
            }

            var featureFlagsLine = File.ReadAllLines(jSonFilePath).Where(x => x?.Trim() is { Length: > 0 });
            var dicResult = featureFlagsLine.ToDictionary(line => line.Split("=")
                .First().Trim(), line => line.Split("=").Last().Trim());

            var jsonString = JsonConvert.SerializeObject(dicResult, new JsonSerializerSettings {
                ContractResolver = new CamelCaseExceptDictionaryKeysResolver()
            });

            return jsonString;
        }
    }
}