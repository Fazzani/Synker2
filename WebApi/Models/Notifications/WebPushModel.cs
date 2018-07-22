namespace hfa.WebApi.Models.Notifications
{
    using System.ComponentModel.DataAnnotations;
    public class WebPushModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Payload { get; set; }

    }

}

