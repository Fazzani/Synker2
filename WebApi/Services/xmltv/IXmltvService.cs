using System;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.WebApi.Services.xmltv
{
    public interface IXmltvService
    {
        Task CreateIndexTvByDateAsync(DateTime dateTime, CancellationToken cancellationToken);
    }
}