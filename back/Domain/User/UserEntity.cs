using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models;

public interface IParentUserReference
{
    public UserId ParentUserId { get; set; }
    public User? ParentUser { get; set; }
}