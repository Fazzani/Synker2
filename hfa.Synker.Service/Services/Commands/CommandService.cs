using hfa.Synker.Service.Entities.Auth;
using hfa.Synker.Services.Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hfa.Synker.Service.Services
{
    public class CommandService : ICommandService
    {
        private SynkerDbContext _dbcontext;
        private ILogger _logger;

        public CommandService(SynkerDbContext synkerDbContext, ILoggerFactory loggerFactory)
        {
            _dbcontext = synkerDbContext;
            _logger = loggerFactory.CreateLogger(nameof(CommandService));
        }

        public async Task<IEnumerable<Command>> GetWebGabCommandAsync(CancellationToken cancellationToken)
        {
            return await _dbcontext.Command
                .Where(c => c.CommandExecutingType == CommandExecutingType.Cron)
                .ToListAsync();
        }
    }
}
