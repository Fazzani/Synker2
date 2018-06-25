namespace hfa.WebApi.Models.Auth
{
    using hfa.Synker.Service.Entities.Auth;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class RegisterModel
    {
        public RegisterModel()
        {
            Roles = new List<Role>();
        }

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

        [Required]
        public List<Role> Roles { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string UserName { get; set; }

        public User Entity
        {
            get
            {
                var userEntity = new User { BirthDay = BirthDay, Email = Email, FirstName = FirstName, Gender = Gender, LastName = LastName, Photo = Photo };
                userEntity.UserRoles = Roles.Select(x => new UserRole { User = userEntity, RoleId = x.Id }).ToList();
                userEntity.ConnectionState = new ConnectionState { Password = Password, UserName = UserName };
                return userEntity;
            }
        }
    }
}
