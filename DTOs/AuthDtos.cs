namespace ToDo_Manager.DTOs;

public class LoginRequest
{
    public string Name { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Name { get; set; }
    public string Password { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
}

public class LogoutRequest
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
}