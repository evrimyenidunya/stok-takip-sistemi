namespace StokTakipSistemi;

public sealed class UserSession
{
    public int Id { get; init; }
    public string Username { get; init; } = "";
    public string Role { get; init; } = "";

    public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
}
