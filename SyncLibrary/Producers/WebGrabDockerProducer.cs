namespace hfa.Synker.batch.Producers
{
    using hfa.Brokers.Messages.Models;
    using hfa.Synker.Service.Services;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class WebGrabDockerProducer : IWebGrabDockerProducer, IDisposable
    {
        static JsonSerializerSettings JsonSerializerSettings =  new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        ILogger _logger;
        private IWebGrabConfigService _webGrabConfigService;
        private IModel _webgrabChannel;
        private string WebGrabQueueName = "synker.webgrab.queue";

        public WebGrabDockerProducer(IWebGrabConfigService webGrabConfigService, ILogger<WebGrabDockerProducer> logger)
        {
            _logger = logger;
            _webGrabConfigService = webGrabConfigService;
        }

        public async Task StartAsync(IConnection connection, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start Webgrab Producer");
            _webgrabChannel = connection.CreateModel();

            _webgrabChannel.QueueDeclare(queue: WebGrabQueueName,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null
                                        );

            _webgrabChannel.QueuePurge(WebGrabQueueName);

            await Task.Delay(5000);

            foreach (var config in await _webGrabConfigService.GetWebGrabToExecuteAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var webGrabNotification = new WebGrabNotification
                {
                    Cron = config.Cron,
                    DockerImage = config.DockerImage,
                    RunnableHost = config.RunnableHost,
                    MountSourcePath = config.MountSourcePath,
                    WebgrabConfigUrl = config.WebgrabConfigUrl
                };

                var jsonified = JsonConvert.SerializeObject(webGrabNotification, JsonSerializerSettings);
                var customerBuffer = Encoding.UTF8.GetBytes(jsonified);

                IBasicProperties props = _webgrabChannel.CreateBasicProperties();
                props.ContentType = "application/json";
                props.DeliveryMode = 2;
                props.AppId = webGrabNotification.AppId;
                props.UserId = webGrabNotification.UserId;
                props.Timestamp = new AmqpTimestamp(unixTime: DateTime.UtcNow.ToUnixTimestamp());

                _webgrabChannel.ExchangeDeclare(WebGrabQueueName, ExchangeType.Direct);
                _webgrabChannel.QueueDeclare(WebGrabQueueName, false, false, false, null);
                _webgrabChannel.QueueBind(WebGrabQueueName, WebGrabQueueName, WebGrabQueueName, null);

                //TODO: Envoyer le message au bon moment

                _webgrabChannel.BasicPublish(exchange: WebGrabQueueName,
                                     routingKey: WebGrabQueueName,
                                     basicProperties: null,
                                     body: customerBuffer);

                _logger.LogInformation($"Queuing new webgrab config {webGrabNotification.WebgrabConfigUrl}");
            }
        }

        public void Dispose()
        {
            _webgrabChannel?.Close();
            _webgrabChannel?.Dispose();
        }
    }
}
