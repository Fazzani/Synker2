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
        ILogger _logger;
        private IWebGrabConfigService _webGrabConfigService;
        private IModel _webgrabChannel;
        private string WebGrabQueueName = "synker.webgrab.queue";

        public WebGrabDockerProducer(IWebGrabConfigService webGrabConfigService, ILogger<WebGrabDockerProducer> logger)
        {
            _logger = logger;
            _webGrabConfigService = webGrabConfigService;
        }

        public void Start(IConnection connection, ManualResetEvent shutdown)
        {
            _webgrabChannel = connection.CreateModel();

            _webgrabChannel.QueueDeclare(queue: WebGrabQueueName,
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null
                                        );
            while (!shutdown.WaitOne())
            {
                foreach (var config in _webGrabConfigService.GetWebGrabToExecuteAsync(CancellationToken.None).GetAwaiter().GetResult())
                {
                    var webGrabNotification = new WebGrabNotification
                    {
                        Cron = config.Cron,
                        DockerImage = config.DockerImage,
                        RunnableHost = config.RunnableHost,
                        MountSourcePath = config.MountSourcePath,
                        WebgrabConfigUrl = config.WebgrabConfigUrl
                    };

                    var jsonified = JsonConvert.SerializeObject(webGrabNotification);
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

                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            _webgrabChannel?.Close();
            _webgrabChannel?.Dispose();
        }
    }
}
