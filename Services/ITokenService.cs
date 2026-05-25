using ToDo_Manager.Models;

namespace ToDo_Manager.Services;

public interface ITokenService
{
    string CreateToken(User user);
}