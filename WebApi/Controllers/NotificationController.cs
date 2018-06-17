using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using hfa.Brokers.Messages.Models;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Models.Notifications;
using Hfa.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly IMessageQueueService _notificationService;
        private readonly VapidKeysOptions _vapidKeysConfig;

        public NotificationController(IMessageQueueService notificationService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
           IElasticConnectionClient elasticConnectionClient, SynkerDbContext context, IOptions<VapidKeysOptions> vapidKeysOptions)
           : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _vapidKeysConfig = vapidKeysOptions.Value;
        }

        /// <summary>
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [ProducesResponseType((int)HttpStatusCode.OK)]
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

        /// <summary>
        /// Push new WebPuch to device id
        /// </summary>
        /// <param name="webPushModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> PostWebPushAsync([FromBody] WebPushModel webPushModel, CancellationToken cancellationToken)
        {
            var device = await _dbContext.Devices.FirstOrDefaultAsync(x => x.Id == webPushModel.Id, cancellationToken);
            if (device == null)
                return NotFound(device);

            var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
            var vapidDetails = new VapidDetails("mailto:synker-team@synker.ovh", _vapidKeysConfig.PublicKey, _vapidKeysConfig.PrivateKey);

            var webPushClient = new WebPushClient();
            webPushClient.SendNotification(pushSubscription, webPushModel.Payload, vapidDetails);
            return Ok();
        }

        /// <summary>
        /// Generate Vapid Keys 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("keys")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetKeysAsync(CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var keys = VapidHelper.GenerateVapidKeys();
                return Ok(keys);
            }, cancellationToken);
        }
    }
}