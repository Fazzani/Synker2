using hfa.synker.batch.MoqServices;
using Hfa.SyncLibrary.Messages;
using Hfa.SyncLibrary.Verbs;
using Moq;
using SyncLibrary;
using System;
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
            using (var client = StartUp.Client)
            {
                var res = SynchElastic.PushXmltvAsync(new PushXmltvVerb
                {
                    ApiUrl = ApiUrl,
                    FilePath = @"data\epg.xmltv"
                }, _messageService, client).GetAwaiter().GetResult();

                //TODO : Fixer ce test d'intégration
                Assert.False(res);
            }
        }
    }
}
