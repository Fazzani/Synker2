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
        private const string WEBGRAB_IMAGE = "synker/webgraboneshoturl";
        ILogger _logger;
        private DockerOptions _dockerOptions;
        private IModel _webgrabChannel;
        private string WebGrabQueueName = Init.IsDev ? "synker.dev.webgrab.queue" : "synker.webgrab.queue";
        private EventHandler<CallbackExceptionEventArgs> eventChannel_CallbackException;

        public WebGrabDockerConsumer(ILogger<NotificationConsumer> logger, IOptions<DockerOptions> dockerOptions)
        {
            _logger = logger;
            _dockerOptions = dockerOptions.Value;
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
                var message = Encoding.UTF8.GetString(body);
                var mail = JsonConvert.DeserializeObject<WebGrabNotification>(message);

                var credentials = new CertificateCredentials(new X509Certificate2(_dockerOptions.CertFilePath, _dockerOptions.CertPassword));
                credentials.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                var cancellationToken = new CancellationTokenSource();

                CreateContainerAsync(credentials, ea, cancellationToken.Token).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                _webgrabChannel.BasicReject(ea.DeliveryTag, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="ea"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task CreateContainerAsync(CertificateCredentials credentials, BasicDeliverEventArgs ea, CancellationToken cancellationToken)
        {
            using (var dockerClient = new DockerClientConfiguration(_dockerOptions.Uri, credentials).CreateClient())
            {
                IList<ContainerListResponse> containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters()
                {
                    All = true
                }, cancellationToken);

                var images = await dockerClient.Images.ListImagesAsync(new ImagesListParameters { MatchName = WEBGRAB_IMAGE });
                if (images.Count == 0)
                {
                    // No image found. Pulling latest ..
                    await dockerClient.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = WEBGRAB_IMAGE, Tag = "latest" }, null, IgnoreProgress.Forever);
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
                    Image = WEBGRAB_IMAGE,
                    Env = new List<string> { "WEBGRAB_CONFIG_URL=https://raw.githubusercontent.com/Fazzani/xmltv/master/WebGrab%2B%2B.config.xml" },
                    HostConfig = new HostConfig
                    {
                        AutoRemove = true,
                        Mounts = new List<Mount>
                        {
                            new Mount{
                            Source = "/c/Users/Heni/Source/Repos/WebGrabDocker/WebGrabFromUrl/tmp",
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
                                        arguments: null);

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