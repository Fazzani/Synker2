namespace hfa.Brokers.Messages.Models
{
    using hfa.Synker.Service.Entities;
    public class WebGrabNotification : NotificationContext
    {
        public WebGrabNotification() : base()
        {

        }

        public WebGrabNotification(string userId) : base(userId)
        {

        }

        /// <summary>
        /// Volume Source Path to mount
        /// </summary>
        public string MountSourcePath { get; set; }

        /// <summary>
        /// Docker image to run
        /// </summary>
        public string DockerImage { get; set; }

        public string WebgrabConfigUrl { get; set; }

        /// <summary>
        /// Where the docker container will be hosted
        /// </summary>
        public Host RunnableHost { get; set; }

        public string Cron { get; set; } = "0 4 * * *";

    }
    
}
