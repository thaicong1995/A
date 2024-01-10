using WebApi.Models;

namespace WebApi.Reposetory.Interface
{
    public interface IUserRepo
    {
        public User GetUserById(int userId);
        User GetUserByActivationToken(string activationToken);
        User GetUserByEmail(string email);
        AcessToken GetValidTokenByUserId(int userId);
    }
}
