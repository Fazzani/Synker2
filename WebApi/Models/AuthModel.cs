﻿using hfa.WebApi.Dal.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models
{
    public class AuthModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string RefreshToken { get; set; }

        public GrantType GrantType { get; set; } = GrantType.Password;
    }
    public enum GrantType : byte
    {
        Password = 0,
        RefreshToken
    }
    public class TokenModel
    {
        [Required]
        public string Token { get; set; }
    }
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

        public List<Role> Roles { get; set; }

        [Required]
        public string Password { get; set; }
        [Required]
        public string UserName { get; set; }

        public User Entity
        {
            get
            {
                var userEntity = new User { BirthDay = BirthDay, Email = Email, FirstName = FirstName, Gender = Gender, LastName = LastName, Photo = Photo, Roles = Roles };
                userEntity.ConnectionState = new ConnectionState { Password = Password, UserName = UserName };
                return userEntity;
            }
        }
    }

    /// <summary>
    /// Reset Password Model
    /// </summary>
    public class ResetModel
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string UserName { get; set; }
    }
}
