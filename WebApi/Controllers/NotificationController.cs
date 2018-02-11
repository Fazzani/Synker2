using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using hfa.Brokers.Messages.Emailing;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Service.Services.Notification;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models.Notifications;
using Hfa.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class NotificationController : BaseController
    {
        private INotificationService _notificationService;

        public NotificationController(INotificationService notificationService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
           IElasticConnectionClient elasticConnectionClient, SynkerDbContext context)
           : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Post([FromBody] NotificationModel notification, CancellationToken cancellationToken)
        {
            if (notification.NotificationType == NotificationTypeEnum.Email)
            {
                await _notificationService.SendMailAsync(new EmailNotification
                {
                    Body = notification.Body,
                    Cc = notification.Cc,
                    Cci = notification.Cci,
                    From = notification.From,
                    Subject = notification.Subject,
                    To = notification.To,
                    FromDisplayName = notification.FromDisplayName,
                    IsBodyHtml = notification.IsBodyHtml,
                    AppId = Assembly.GetExecutingAssembly().FullName,
                    UserId = UserId.Value.ToString()
                }, cancellationToken);
            }

            return Ok();
        }
    }
}