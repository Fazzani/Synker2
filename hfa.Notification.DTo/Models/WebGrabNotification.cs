using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Brokers.Messages.Models
{
    public class WebGrabNotification : NotificationContext
    {
        public WebGrabNotification() : base()
        {

        }

        public WebGrabNotification(string userId) : base(userId)
        {

        }

        /// <summary>
        /// WebGrabFilePath
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        /// Docker image to run
        /// </summary>
        public string DockerImage { get; set; }
    }
}
