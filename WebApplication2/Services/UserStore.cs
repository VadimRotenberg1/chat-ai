using System.Security.Cryptography;
using System.Text;
using WebApplication2.Contracts;

namespace WebApplication2.Services;

/// <summary>
/// Demo in-memory user store. Replace with a real persistence layer for production.
/// </summary>
public sealed class UserStore
{
    private sealed record StoredUser(string Id, string Username, string DisplayName, string Email, string PasswordHash, string[] Roles);

    private readonly Dictionary<string, StoredUser> _usersByName;

    public UserStore()
    {
        _usersByName = new Dictionary<string, StoredUser>(StringComparer.OrdinalIgnoreCase)
        {
            ["alice"] = new(
                Id: "1",
                Username: "alice",
                DisplayName: "Alice Anderson",
                Email: "alice@example.com",
                PasswordHash: Hash("password123"),
                Roles: ["user"]),
            ["admin"] = new(
                Id: "2",
                Username: "admin",
                DisplayName: "Site Admin",
                Email: "admin@example.com",
                PasswordHash: Hash("admin123"),
                Roles: ["user", "admin"]),
        };
    }

    public UserProfile? Authenticate(string username, string password)
    {
        if (!_usersByName.TryGetValue(username, out var user))
        {
            return null;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(user.PasswordHash),
            Encoding.UTF8.GetBytes(Hash(password)))
            ? ToProfile(user)
            : null;
    }

    public UserProfile? FindById(string id) =>
        _usersByName.Values.FirstOrDefault(u => u.Id == id) is { } user ? ToProfile(user) : null;

    private static UserProfile ToProfile(StoredUser user) =>
        new(user.Id, user.Username, user.DisplayName, user.Email, user.Roles);

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
