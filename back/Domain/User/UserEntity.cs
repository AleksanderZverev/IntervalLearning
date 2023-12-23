using Domain.User.ValueObjects;

namespace Domain.User;

public interface IParentUserReference
{
    public UserId ParentUserId { get; set; }
    public User? ParentUser { get; set; }
}