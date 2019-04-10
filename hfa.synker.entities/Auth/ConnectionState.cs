//namespace hfa.Synker.Service.Entities.Auth
//{
//    using System;
//    using System.ComponentModel.DataAnnotations;

//    public class ConnectionState : EntityBase
//    {
//        [Required]
//        [MaxLength(512)]
//        public string UserName { get; set; }

//        [Required]
//        [MaxLength(512)]
//        public string Password { get; set; }

//        public DateTime LastConnection { get; set; } = DateTime.Now;

//        [MaxLength(255)]
//        public string RefreshToken { get; set; }

//        public string AccessToken { get; set; }

//        public bool Disabled { get; set; } = false;

//        public bool Approved { get; set; } = true;
//    }
//}
