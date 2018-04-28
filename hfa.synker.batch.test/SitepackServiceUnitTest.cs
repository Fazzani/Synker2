using hfa.Synker.Service.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace hfa.synker.batch.test
{
    public class SitepackServiceUnitTest
    {
        [Fact]
        [Trait("Category", "Unit")]
        public async Task WebgrabConfigBySitePackTest()
        {
            const string sitepackUrl = "https://raw.githubusercontent.com/SilentButeo2/webgrabplus-siteinipack/master/siteini.pack/Australia/fetchtv.com.au.channels.xml";
            var moqLoggerFactory = new  Moq.Mock<ILoggerFactory>();
            
            var sitePackService = new SitePackService(null, null, moqLoggerFactory.Object);
            var result = await sitePackService.WebgrabConfigBySitePackAsync(sitepackUrl,"tmp.xml", CancellationToken.None);
            
            Assert.NotNull(result);
        }
    }
}
