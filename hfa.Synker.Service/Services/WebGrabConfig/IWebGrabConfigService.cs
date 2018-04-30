using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using hfa.synker.entities;

namespace hfa.Synker.Service.Services
{
    public interface IWebGrabConfigService
    {
        Task<IEnumerable<WebGrabConfigDocker>> GetWebGrabToExecuteAsync(CancellationToken cancellationToken = default);
    }
}