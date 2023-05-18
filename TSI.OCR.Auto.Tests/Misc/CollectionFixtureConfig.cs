using System;
using TSI.OCR.Auto.Tests.Misc.LogHTML;
using Xunit;

namespace TSI.OCR.Auto.Tests.Misc
{
    public class CollectionFixtureConfig
    {
        [CollectionDefinition("HtmlCollection")]
        public class CollectionDefinition : ICollectionFixture<HtmlLogCollectFix>
        {
            public CollectionDefinition()
            {
                Environment.SetEnvironmentVariable("TEST_CONFIG_FILE", "Windows.config");
            }
        }
    }

    public class HtmlLogCollectFix : IDisposable
    {
        
        public void Dispose()
        {
            Console.WriteLine("Start HtmlLogCollectingFix");
            LogHtmlStaticHtmlError.CreatHtmlReport();
            Console.WriteLine("Dispose HtmlLogCollectingFix");
        }
    }
}