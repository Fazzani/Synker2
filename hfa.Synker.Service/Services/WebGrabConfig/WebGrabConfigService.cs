namespace hfa.Synker.Service.Services
{
    using hfa.synker.entities;
    using hfa.Synker.Services.Dal;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class WebGrabConfigService : IWebGrabConfigService
    {
        private SynkerDbContext _dbcontext;
        private ILogger _logger;

        public WebGrabConfigService(SynkerDbContext synkerDbContext, ILoggerFactory loggerFactory)
        {
            _dbcontext = synkerDbContext;
            _logger = loggerFactory.CreateLogger(nameof(WebGrabConfigService));

        }
        public async Task<IEnumerable<WebGrabConfigDocker>> GetWebGrabToExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dbcontext.WebGrabConfigDockers.Include(x => x.RunnableHost).ToListAsync();
        }

    }
}
