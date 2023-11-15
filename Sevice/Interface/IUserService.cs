using WebApi.Dto;
using WebApi.Models;

namespace WebApi.Sevice.Interface
{
    public interface IUserService
    {
        List<User> GetUsers();
        User RegisterUser(User user);
        bool SendPasswordResetEmail(string email);
        bool ActivateUser(string activationToken);
        bool LogoutUser(int userId);
        User LoginUser(UserDto userDto);
    }
}
