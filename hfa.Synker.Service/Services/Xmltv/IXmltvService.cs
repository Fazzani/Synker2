using System;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Services.Xmltv
{
    public interface IXmltvService
    {
        Task CreateIndexTvByDateAsync(DateTime dateTime, CancellationToken cancellationToken);
    }
}