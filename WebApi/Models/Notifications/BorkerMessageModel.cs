namespace hfa.WebApi.Models.Notifications
{
    using System.ComponentModel.DataAnnotations;

    public class BorkerMessageModel
    {
        public BrokerMessageType BrokerMessageType { get; set; }

        [Required]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }

        /// <summary>
        /// User Id or group Id target
        /// </summary>
        public int Target { get; set; }

        public object Extra { get; set; }

        public override string ToString() => $"BrokerMessageType: {BrokerMessageType}, Id :{Id}, Message : {Message}";
    }

    public enum BrokerMessageType : uint
    {
        Exception = 0,
        PlaylistDiff = 1,
    }
}

