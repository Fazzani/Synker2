namespace hfa.synker.entities
{
    using hfa.Synker.Service.Entities;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WebGrabConfigDocker : EntityBase
    {
        /// <summary>
        /// Volume Source Path to mount
        /// </summary>
        public string MountSourcePath { get; set; } = "$PWD";

        /// <summary>
        /// Docker image to run
        /// </summary>
        [Required]
        public string DockerImage { get; set; }

        [Required]
        public string WebgrabConfigUrl { get; set; }

        /// <summary>
        /// Where the docker container will be hosted
        /// </summary>
        [ForeignKey(nameof(HostId))]
        public virtual Host RunnableHost { get; set; }

        public int HostId { get; set; }

        [Required]
        public string Cron { get; set; } = "0 4 * * *";

    }
}
