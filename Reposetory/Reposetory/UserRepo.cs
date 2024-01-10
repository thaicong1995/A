using WebApi.Models;
using WebApi.Models.Enum;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;

namespace WebApi.Reposetory.Reposetory
{
    public class UserRepo : IUserRepo
    {
        private readonly MyDb _myDb;

        public UserRepo(MyDb myDb)
        {
            _myDb = myDb;
        }

        public User GetUserById(int userId)
        {
            return _myDb.Users.SingleOrDefault(s => s.Id == userId);
        }

        public User GetUserByActivationToken(string activationToken)
        {
            return _myDb.Users.SingleOrDefault(u => u.ActivationToken == activationToken);
        }

        public User GetUserByEmail(string email)
        {
            return _myDb.Users.SingleOrDefault(u => u.Email == email);
        }

        public AcessToken GetValidTokenByUserId(int userId)
        {
            return _myDb.AccessTokens.FirstOrDefault(t => t.UserID == userId && t.statusToken == StatusToken.Valid);
        }

        public Shop GetShopByUser(int userId)
        {
            return _myDb.Shops.FirstOrDefault(s => s.UserId == userId);
        }
    }
}
