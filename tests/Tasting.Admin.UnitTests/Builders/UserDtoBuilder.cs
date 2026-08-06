using Tasting.Admin.Features.Identity.Models;

namespace Tasting.Admin.UnitTests.Builders;

public class UserDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _firstName = "John";
    private string _lastName = "Doe";
    private string _email = "john.doe@example.com";
    private string _role = "User";
    private string _status = "Active";

    public UserDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserDtoBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public UserDtoBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public UserDtoBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserDtoBuilder WithRole(string role)
    {
        _role = role;
        return this;
    }

    public UserDtoBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public UserDto Build()
    {
        return new UserDto(_id, _firstName, _lastName, _email, _role, _status);
    }

    public static UserDtoBuilder Default()
    {
        return new UserDtoBuilder();
    }

    public static UserDtoBuilder Admin()
    {
        return new UserDtoBuilder().WithRole("Admin");
    }

    public static UserDtoBuilder Inactive()
    {
        return new UserDtoBuilder().WithStatus("Inactive");
    }
}
