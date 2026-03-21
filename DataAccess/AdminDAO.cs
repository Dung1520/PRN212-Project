using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;

namespace DataAccess
{
    public class AdminDAO
    {
        public Admin? GetByEmail(string email)
        {
            using var context = DbContextFactory.CreateDbContext();
            return context.Admins
                .FirstOrDefault(x => x.Email == email && x.IsActive);
        }
    }
}
