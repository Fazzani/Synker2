namespace hfa.Synker.batch.Consumers
{
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using Docker.DotNet.X509;
    using hfa.Brokers.Messages.Models;
    using hfa.Notification.Brokers.Consumers;
    using Hfa.SyncLibrary;
    using Hfa.SyncLibrary.Infrastructure;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NCrontab;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class WebGrabDockerConsumer : IWebGrabDockerConsumer, IDisposable
    {
        ILogger _logger;
        private IModel _webgrabChannel;
        private string WebGrabQueueName = "synker.webgrab.queue";
        private EventHandler<CallbackExceptionEventArgs> eventChannel_CallbackException;

        public WebGrabDockerConsumer(ILogger<NotificationConsumer> logger)
        {
            _logger = logger;
            eventChannel_CallbackException = new EventHandler<CallbackExceptionEventArgs>(Channel_CallbackException);
        }

        private void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, e.Exception.Message);
        }

        private void ReceivedMessage(object model, BasicDeliverEventArgs ea)
        {
            try
            {
                _logger.LogInformation($"New WebGrab config poped from the queue {WebGrabQueueName}");

                var body = ea.Body;
                var messageString = Encoding.UTF8.GetString(body);
                var webGrabNotificationMessage = JsonConvert.DeserializeObject<WebGrabNotification>(messageString);

                var cron = CrontabSchedule.Parse(webGrabNotificationMessage.Cron);
                var cronOcc = cron.GetNextOccurrence(DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm");
                if (cronOcc == DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"))
                {
                    var cancellationToken = new CancellationTokenSource();
                    CreateContainerAsync(ea, webGrabNotificationMessage, cancellationToken.Token).GetAwaiter().GetResult();
                }
                else
                {
#if DEBUG
                    Thread.Sleep(6000);
#endif
                    _logger.LogInformation($"Requeing Message {webGrabNotificationMessage.WebgrabConfigUrl}");
                    _webgrabChannel.BasicReject(ea.DeliveryTag, true);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                _webgrabChannel.BasicReject(ea.DeliveryTag, true);
            }
        }

        /// <summary>
        /// Run docker container    
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="ea"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task CreateContainerAsync(BasicDeliverEventArgs ea, WebGrabNotification webGrabNotificationMessage, CancellationToken cancellationToken)
        {
            CertificateCredentials credentials = null;
            if (webGrabNotificationMessage.RunnableHost.Authentication != null
                && !string.IsNullOrEmpty(webGrabNotificationMessage.RunnableHost.Authentication.CertPath))
            {
                credentials = new CertificateCredentials(new X509Certificate2(webGrabNotificationMessage.RunnableHost.Authentication.CertPath, webGrabNotificationMessage.RunnableHost.Authentication.Password));
                credentials.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            }

            using (var dockerClient = new DockerClientConfiguration(webGrabNotificationMessage.RunnableHost.AdressUri, credentials).CreateClient())
            {
                IList<ContainerListResponse> containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters()
                {
                    All = true
                }, cancellationToken);

                var images = await dockerClient.Images.ListImagesAsync(new ImagesListParameters { MatchName = webGrabNotificationMessage.DockerImage });
                if (images.Count == 0)
                {
                    // No image found. Pulling latest ..
                    await dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
                    {
                        FromImage = webGrabNotificationMessage.DockerImage,
                        Tag = "latest"
                    }, null, IgnoreProgress.Forever);
                }

                /*
                 * docker run -it --rm -e WEBGRAB_CONFIG_URL=https://raw.githubusercontent.com/Fazzani/xmltv/master/WebGrab++.config.xml \
                 * -v "$(pwd):/data" synker/webgraboneshoturl:latest
                 * /mnt/nfs/webgrab/xmltv
                 * 
                 * on windows 
                 * -------
                 * docker run -it --rm -e "WEBGRAB_CONFIG_URL=https://raw.githubusercontent.com/Fazzani/xmltv/master/WebGrab%2B%2B.config.xml" \
                 * -e DEBUG=1 -v /$(pwd):/data synker/webgraboneshoturl:latest
                 * To see for sharing volume : https://github.com/rocker-org/rocker/wiki/Sharing-files-with-host-machine
                 * example code :  https://gist.github.com/yreynhout/c7569833d78c8db255ec8d7703bb3ae5
                 */
                var createdContainer = await dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = webGrabNotificationMessage.DockerImage,
                    Env = new List<string> { $"WEBGRAB_CONFIG_URL={webGrabNotificationMessage.WebgrabConfigUrl}", "DEBUG=1" },
                    HostConfig = new HostConfig
                    {
                        AutoRemove = !Init.IsDev,
                        Mounts = new List<Mount>
                        {
                            new Mount{
                            Source = webGrabNotificationMessage.MountSourcePath,
                            Target = "/data",
                            ReadOnly = false,
                            Type = "bind"
                        }}
                    }
                }, cancellationToken);

                var containerIsStarted = await dockerClient
                    .Containers
                    .StartContainerAsync(createdContainer.ID, new ContainerStartParameters(), cancellationToken);

                //Monitoring container executing and waiting for the end
                _logger.LogInformation($"container started id : {createdContainer.ID}");

                //ack message
                _webgrabChannel.BasicAck(ea.DeliveryTag, true);
            }
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

            _webgrabChannel.CallbackException += eventChannel_CallbackException;
            var mailConsumer = new EventingBasicConsumer(_webgrabChannel);

            mailConsumer.Received += ReceivedMessage;

            _webgrabChannel.BasicConsume(queue: WebGrabQueueName,
                                        autoAck: false,
                                        consumer: mailConsumer);

            while (!shutdown.WaitOne())
            {
                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            _webgrabChannel?.Close();
            _webgrabChannel?.Dispose();
        }

        private class IgnoreProgress : IProgress<JSONMessage>
        {
            public static readonly IProgress<JSONMessage> Forever = new IgnoreProgress();

            public void Report(JSONMessage value) { }
        }
    }
}