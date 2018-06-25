namespace hfa.WebApi.Models.Admin
{
    using hfa.Synker.Service.Entities.Auth;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class UserModel
    {
        public UserModel()
        {

        }

        public UserModel(User user)
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            UpdatedDate = user.UpdatedDate;
            CreatedDate = user.CreatedDate;
            Id = user.Id;
            Photo = user.Photo;
            BirthDay = user.BirthDay;
            Gender = user.Gender;
            Roles = user.Roles;
            ConnectionState = user.ConnectionState;
        }

        public int Id { get; set; }

        [MaxLength(64)]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(64)]
        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public string Photo { get; set; }

        public DateTime BirthDay { get; set; }

        public GenderTypeEnum Gender { get; set; } = GenderTypeEnum.Mr;

        public ConnectionState ConnectionState { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
