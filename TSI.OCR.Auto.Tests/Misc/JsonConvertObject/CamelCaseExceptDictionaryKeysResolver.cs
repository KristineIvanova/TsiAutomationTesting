using System;
using Newtonsoft.Json.Serialization;

namespace TSI.OCR.Auto.Tests.Misc.JsonConvertObject {
    class CamelCaseExceptDictionaryKeysResolver: CamelCasePropertyNamesContractResolver {
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType) {
            var contract = base.CreateDictionaryContract(objectType);
            contract.DictionaryKeyResolver = propertyName => propertyName;
            return contract;
        }
    }
}