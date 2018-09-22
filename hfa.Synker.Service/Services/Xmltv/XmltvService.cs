using hfa.PlaylistBaseLibrary.Entities;
using Nest;
using PlaylistBaseLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Services.Xmltv
{
    public class XmltvService : IXmltvService
    {
        private IElasticClient _elasticClient;

        public XmltvService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        /// <summary>
        /// Create index Tv by date
        /// </summary>
        /// <param name="dateTime"></param>
        public async Task CreateIndexTvByDateAsync(DateTime dateTime, CancellationToken cancellationToken = default(CancellationToken))
        {
            var keywordProperty = new PropertyName("keyword");

            string index = $"xmltv-{dateTime.ToString("d")}";
            if (!(await _elasticClient.IndexExistsAsync(index, cancellationToken: cancellationToken)).Exists)
            {
                var response = await _elasticClient.CreateIndexAsync(index, c => c
                  .Mappings(x => x.Map<tv>(tv => tv.Properties(p =>
                     p.Object<tvChannel>(tvc => tvc.AutoMap())
                      .Object<tvProgramme>(tvp => tvp.AutoMap())
                       ))), cancellationToken: cancellationToken);
            }
        }
    }
}
