using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using hfa.Synker.Service.Entities.Auth;

namespace hfa.Synker.Service.Services
{
    public interface ICommandService
    {
        Task<IEnumerable<Command>> GetWebGabCommandAsync(CancellationToken cancellationToken);
    }
}