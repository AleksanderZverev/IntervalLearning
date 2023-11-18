using System.Diagnostics;
using System.Text.Json.Serialization;
using DB.Models;
using Domain.User.ValueObjects;
using FluentResults;

namespace Domain.User.Entities
{
    public class UserPassword : Entity<UserId>, IParentUserReference
    {
        protected UserPassword(UserId parentUserId) : base(parentUserId)
        {
            ParentUserId = parentUserId;
        }

        public UserId ParentUserId { get; set; }
        public User? ParentUser { get; set; }
        
        [JsonIgnore]
        public string? PasswordHash { get; init; }

        public static Result<UserPassword> Create(UserId id, string passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash))
            {
                Debug.Fail("Passed empty password hash");
                return Result.Fail("Incorrect password hash");
            }

            return new UserPassword(id)
            {
                PasswordHash = passwordHash
            };
        }
    }
}
