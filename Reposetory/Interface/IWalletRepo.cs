using WebApi.Models;

namespace WebApi.Reposetory.Interface
{
    public interface IWalletRepo
    {
        Wallet GetWalletByUserId(int userId);
    }
}
