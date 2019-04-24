using hfa.Brokers.Messages.Models;
using hfa.Synker.Service.Elastic;
using hfa.Synker.Service.Services;
using hfa.Synker.Service.Services.Elastic;
using hfa.Synker.Services.Dal;
using hfa.WebApi.Common;
using hfa.WebApi.Common.Auth;
using hfa.WebApi.Common.Filters;
using hfa.WebApi.Hubs;
using hfa.WebApi.Models.Notifications;
using Hfa.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WebPush;

namespace hfa.WebApi.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Authorize(AuthenticationSchemes = Common.Auth.Authentication.AuthSchemes)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly IMessageQueueService _notificationService;
        private readonly VapidKeysOptions _vapidKeysConfig;
        private readonly IHubContext<NotificationHub> _notifcationHubContext;

        public NotificationController(IMessageQueueService notificationService, IOptions<ElasticConfig> config, ILoggerFactory loggerFactory,
           IElasticConnectionClient elasticConnectionClient, SynkerDbContext context, IHubContext<NotificationHub> notifcationHubContext, IOptions<VapidKeysOptions> vapidKeysOptions)
           : base(config, loggerFactory, elasticConnectionClient, context)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _vapidKeysConfig = vapidKeysOptions.Value;
            _notifcationHubContext = notifcationHubContext;
        }

        /// <summary>
        /// Create new Notification
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> Post([FromBody] NotificationModel notification, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { UserId }, cancellationToken);
            if (user == null) return BadRequest($"User {this.UserEmail} not found");

            switch (notification.NotificationType)
            {
                case NotificationTypeEnum.Email:
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
                        UserId = user.Id.ToString(),
                        TemplateId = "d-46b66e30d388448d955ec0b73630eb21",
                        TemplateData = new
                        {
                            header = "Reset Password",
                            text = notification.Body,
                            c2a_link = "",
                            c2a_button = "Reset Password"
                        }
                    }, cancellationToken);
                    break;
                case NotificationTypeEnum.Sms:
                    break;
                case NotificationTypeEnum.PushBrowser:
                    break;
                case NotificationTypeEnum.PushMobile:
                    break;
                default:
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
                        UserId = user.Id.ToString()
                    }, cancellationToken);
                    break;
            }
            return Ok();
        }

        /// <summary>
        /// Push new WebPuch to device id
        /// </summary>
        /// <param name="webPushModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("webpush")]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = AuthorizePolicies.FULLACCESS)]
        public async Task<IActionResult> PostWebPushAsync([FromBody] WebPushModel webPushModel, CancellationToken cancellationToken = default)
        {
            synker.entities.Notifications.Device device = await _dbContext.Devices.FirstOrDefaultAsync(x => x.Id == webPushModel.Id, cancellationToken);
            if (device == null)
            {
                return NotFound(device);
            }

            PushSubscription pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
            VapidDetails vapidDetails = new VapidDetails("mailto:synker-team@synker.ovh", _vapidKeysConfig.PublicKey, _vapidKeysConfig.PrivateKey);

            new WebPushClient().SendNotification(pushSubscription, webPushModel.Payload, vapidDetails);
            return Ok();
        }

        /// <summary>
        /// Generate Vapid Keys 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("keys")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Policy = AuthorizePolicies.READER)]
        public async Task<IActionResult> GetKeysAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                VapidDetails keys = VapidHelper.GenerateVapidKeys();
                return Ok(keys);
            }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Test Web push notification
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Route("push")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> GetWebPushNotification([FromBody]BorkerMessageModel message, CancellationToken cancellationToken = default)
        {
            await _notifcationHubContext.Clients.All.SendAsync("SendMessage", User.Identity.Name, message.Message, cancellationToken);
            return Ok();
        }
    }
}