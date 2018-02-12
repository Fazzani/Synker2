using hfa.synker.batch.MoqServices;
using Hfa.SyncLibrary.Verbs;
using Moq;
using SyncLibrary;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace hfa.synker.batch.test
{
    public class PushXmltvUnitTest
    {
        private const string ApiUrl = "http://localhost:56800/api/v1/xmltv/uploadjson";
        private MoqMessageService _messageService;

        public PushXmltvUnitTest()
        {
            _messageService = new MoqMessageService();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void PushXmlvTest()
        {
            var appData = new Hfa.SyncLibrary.Infrastructure.ApplicationConfigData { };
            using (var client = StartUp.Client)
            {
                var res = SynchElastic.PushXmltvAsync(new PushXmltvVerb
                {
                    ApiUrl = ApiUrl,
                    FilePath = Path.Combine(Directory.GetCurrentDirectory(), "epg.xmltv")
                }, _messageService, client, appData).GetAwaiter().GetResult();

                //TODO : Fixer ce test d'intégration
                Assert.False(res);
            }
        }
    }
}
