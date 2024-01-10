using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.MyDbContext;
using WebApi.Reposetory.Interface;

namespace WebApi.Reposetory.Reposetory
{
    public class DiscountRepo : IDiscountRepo
    {
        private readonly MyDb _myDb;

        public DiscountRepo(MyDb myDb)
        {
            _myDb = myDb;
        }
        public Discounts GetbyIdDiscount(int id)
        {
            return _myDb.Discounts.FirstOrDefault(d => d.Id == id);
        }

        public List<Discounts> GetUnusedDiscountsByUserId(int userId)
        {
            var listDiscounts = from d in _myDb.Discounts
                                join du in _myDb.DiscountUsages on new { DiscountId = d.Id, UserId = userId } equals new { DiscountId = du.DiscountId, UserId = du.UserId } into gj
                                from subdu in gj.DefaultIfEmpty()
                                where subdu == null
                                select d;

            return listDiscounts.ToList();
        }
    }
}
