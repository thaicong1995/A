using WebApi.Models;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;

namespace WebApi.Reposetory.Reposetory
{
    public class WalletRepo : IWalletRepo
    {
        private readonly MyDb _myDb;

        public WalletRepo(MyDb myDb)
        {
            _myDb = myDb;
        }
        public Wallet GetWalletByUserId(int userId)
        {
            return _myDb.Wallets.FirstOrDefault(w => w.UserId == userId);
        }
    }
}
