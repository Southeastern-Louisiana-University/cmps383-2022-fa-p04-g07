namespace FA22.P04.Tests.Web.Dtos;

internal class UserDto : PasswordGuard
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string[]? Roles { get; set; }
}